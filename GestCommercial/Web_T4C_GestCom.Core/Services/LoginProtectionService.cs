using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace Web_T4C_GestCom.Services;

public sealed class LoginProtectionOptions
{
    public int LoginWindowSeconds { get; set; } = 900;
    public int IpWindowSeconds { get; set; } = 900;

    public int MaxFailuresPerLoginWindow { get; set; } = 8;
    public int MaxFailuresPerIpWindow { get; set; } = 20;

    public int LoginLockoutThreshold { get; set; } = 6;
    public int IpLockoutThreshold { get; set; } = 12;

    public int ProgressiveBackoffStartFailures { get; set; } = 3;
    public int BackoffBaseSeconds { get; set; } = 2;
    public int BackoffMaxSeconds { get; set; } = 120;

    public int LoginLockoutSeconds { get; set; } = 900;
    public int IpLockoutSeconds { get; set; } = 600;

    public int AlertOnLoginFailures { get; set; } = 4;
    public int AlertOnIpFailures { get; set; } = 10;

    internal void Normalize()
    {
        LoginWindowSeconds = Math.Max(60, LoginWindowSeconds);
        IpWindowSeconds = Math.Max(60, IpWindowSeconds);

        MaxFailuresPerLoginWindow = Math.Max(2, MaxFailuresPerLoginWindow);
        MaxFailuresPerIpWindow = Math.Max(2, MaxFailuresPerIpWindow);

        LoginLockoutThreshold = Math.Max(2, LoginLockoutThreshold);
        IpLockoutThreshold = Math.Max(2, IpLockoutThreshold);

        ProgressiveBackoffStartFailures = Math.Max(2, ProgressiveBackoffStartFailures);
        BackoffBaseSeconds = Math.Max(1, BackoffBaseSeconds);
        BackoffMaxSeconds = Math.Max(BackoffBaseSeconds, BackoffMaxSeconds);

        LoginLockoutSeconds = Math.Max(30, LoginLockoutSeconds);
        IpLockoutSeconds = Math.Max(30, IpLockoutSeconds);

        AlertOnLoginFailures = Math.Max(2, AlertOnLoginFailures);
        AlertOnIpFailures = Math.Max(2, AlertOnIpFailures);
    }
}

public sealed record LoginProtectionDecision(
    bool IsBlocked,
    DateTimeOffset? RetryAfterUtc,
    string ReasonCode,
    IReadOnlyList<string> Alerts)
{
    public int GetRetryAfterSeconds(DateTimeOffset nowUtc)
    {
        if (!RetryAfterUtc.HasValue)
            return 0;

        return Math.Max(1, (int)Math.Ceiling((RetryAfterUtc.Value - nowUtc).TotalSeconds));
    }
}

public interface ILoginProtectionService
{
    LoginProtectionDecision Evaluate(string login, string ipAddress, DateTimeOffset nowUtc);
    LoginProtectionDecision RegisterFailure(string login, string ipAddress, DateTimeOffset nowUtc);
    void RegisterSuccess(string login, string ipAddress);
}

public sealed class LoginProtectionService : ILoginProtectionService
{
    private readonly LoginProtectionOptions _options;

    private readonly ConcurrentDictionary<string, AttemptState> _loginStates =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly ConcurrentDictionary<string, AttemptState> _ipStates =
        new(StringComparer.Ordinal);

    public LoginProtectionService(IOptions<LoginProtectionOptions> options)
    {
        _options = options.Value ?? new LoginProtectionOptions();
        _options.Normalize();
    }

    public LoginProtectionDecision Evaluate(string login, string ipAddress, DateTimeOffset nowUtc)
    {
        var loginKey = NormalizeLogin(login);
        var ipKey = NormalizeIp(ipAddress);
        return EvaluateCore(loginKey, ipKey, nowUtc);
    }

    public LoginProtectionDecision RegisterFailure(string login, string ipAddress, DateTimeOffset nowUtc)
    {
        var loginKey = NormalizeLogin(login);
        var ipKey = NormalizeIp(ipAddress);

        var alerts = new List<string>(4);

        UpdateFailureState(
            _loginStates,
            loginKey,
            nowUtc,
            _options.LoginWindowSeconds,
            _options.MaxFailuresPerLoginWindow,
            _options.LoginLockoutThreshold,
            _options.LoginLockoutSeconds,
            _options.AlertOnLoginFailures,
            "login",
            alerts);

        UpdateFailureState(
            _ipStates,
            ipKey,
            nowUtc,
            _options.IpWindowSeconds,
            _options.MaxFailuresPerIpWindow,
            _options.IpLockoutThreshold,
            _options.IpLockoutSeconds,
            _options.AlertOnIpFailures,
            "ip",
            alerts);

        var decision = EvaluateCore(loginKey, ipKey, nowUtc);
        if (alerts.Count == 0)
            return decision;

        var uniqueAlerts = alerts.Distinct(StringComparer.Ordinal).ToArray();
        return decision with { Alerts = uniqueAlerts };
    }

    public void RegisterSuccess(string login, string ipAddress)
    {
        var loginKey = NormalizeLogin(login);
        var ipKey = NormalizeIp(ipAddress);

        _loginStates.TryRemove(loginKey, out _);

        if (_ipStates.TryGetValue(ipKey, out var ipState))
        {
            lock (ipState)
            {
                ipState.ConsecutiveFailures = 0;
                ipState.BackoffUntilUtc = null;
                ipState.AlertThresholdRaised = false;
            }
        }
    }

    private LoginProtectionDecision EvaluateCore(string loginKey, string ipKey, DateTimeOffset nowUtc)
    {
        var loginUntil = GetActiveBlockUntil(_loginStates, loginKey, nowUtc, _options.LoginWindowSeconds);
        var ipUntil = GetActiveBlockUntil(_ipStates, ipKey, nowUtc, _options.IpWindowSeconds);

        if (!loginUntil.HasValue && !ipUntil.HasValue)
            return new LoginProtectionDecision(false, null, "none", Array.Empty<string>());

        if (ipUntil.HasValue && (!loginUntil.HasValue || ipUntil.Value >= loginUntil.Value))
            return new LoginProtectionDecision(true, ipUntil, "ip", Array.Empty<string>());

        return new LoginProtectionDecision(true, loginUntil, "login", Array.Empty<string>());
    }

    private void UpdateFailureState(
        ConcurrentDictionary<string, AttemptState> states,
        string key,
        DateTimeOffset nowUtc,
        int windowSeconds,
        int maxFailuresPerWindow,
        int lockoutThreshold,
        int lockoutSeconds,
        int alertThreshold,
        string scope,
        List<string> alerts)
    {
        var state = states.GetOrAdd(key, _ => new AttemptState());

        lock (state)
        {
            ResetWindowIfExpired(state, nowUtc, windowSeconds);

            state.AttemptsInWindow++;
            state.ConsecutiveFailures++;

            if (state.ConsecutiveFailures >= alertThreshold && !state.AlertThresholdRaised)
            {
                alerts.Add($"{scope}.threshold");
                state.AlertThresholdRaised = true;
            }

            var exceededWindow = state.AttemptsInWindow >= maxFailuresPerWindow;
            var exceededConsecutive = state.ConsecutiveFailures >= lockoutThreshold;
            if (exceededWindow || exceededConsecutive)
            {
                var lockoutUntil = nowUtc.AddSeconds(lockoutSeconds);
                if (!state.LockoutUntilUtc.HasValue || state.LockoutUntilUtc.Value < lockoutUntil)
                    state.LockoutUntilUtc = lockoutUntil;

                state.BackoffUntilUtc = null;

                if (!state.LockoutAlertRaised)
                {
                    alerts.Add($"{scope}.lockout");
                    state.LockoutAlertRaised = true;
                }

                return;
            }

            if (state.ConsecutiveFailures >= _options.ProgressiveBackoffStartFailures)
            {
                var exponent = Math.Min(state.ConsecutiveFailures - _options.ProgressiveBackoffStartFailures, 16);
                var backoffSeconds = Math.Min(_options.BackoffMaxSeconds, _options.BackoffBaseSeconds * (1 << exponent));
                var backoffUntil = nowUtc.AddSeconds(backoffSeconds);
                if (!state.BackoffUntilUtc.HasValue || state.BackoffUntilUtc.Value < backoffUntil)
                    state.BackoffUntilUtc = backoffUntil;
            }
        }
    }

    private static DateTimeOffset? GetActiveBlockUntil(
        ConcurrentDictionary<string, AttemptState> states,
        string key,
        DateTimeOffset nowUtc,
        int windowSeconds)
    {
        if (!states.TryGetValue(key, out var state))
            return null;

        var remove = false;
        DateTimeOffset? blockedUntil;

        lock (state)
        {
            ResetWindowIfExpired(state, nowUtc, windowSeconds);

            if (state.BackoffUntilUtc.HasValue && state.BackoffUntilUtc.Value <= nowUtc)
                state.BackoffUntilUtc = null;

            if (state.LockoutUntilUtc.HasValue && state.LockoutUntilUtc.Value <= nowUtc)
                state.LockoutUntilUtc = null;

            blockedUntil = MaxUntil(state.BackoffUntilUtc, state.LockoutUntilUtc);

            remove = blockedUntil is null
                && state.AttemptsInWindow == 0
                && state.ConsecutiveFailures == 0;
        }

        if (remove)
            states.TryRemove(key, out _);

        return blockedUntil;
    }

    private static DateTimeOffset? MaxUntil(DateTimeOffset? a, DateTimeOffset? b)
    {
        if (!a.HasValue) return b;
        if (!b.HasValue) return a;
        return a.Value >= b.Value ? a : b;
    }

    private static void ResetWindowIfExpired(AttemptState state, DateTimeOffset nowUtc, int windowSeconds)
    {
        if (state.WindowStartUtc == DateTimeOffset.MinValue)
        {
            state.WindowStartUtc = nowUtc;
            return;
        }

        if ((nowUtc - state.WindowStartUtc).TotalSeconds <= windowSeconds)
            return;

        state.WindowStartUtc = nowUtc;
        state.AttemptsInWindow = 0;
        state.ConsecutiveFailures = 0;
        state.BackoffUntilUtc = null;
        state.AlertThresholdRaised = false;
        state.LockoutAlertRaised = false;
    }

    private static string NormalizeLogin(string login)
    {
        var normalized = (login ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized)
            ? "anonymous"
            : normalized.ToLowerInvariant();
    }

    private static string NormalizeIp(string ipAddress)
    {
        var normalized = (ipAddress ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized)
            ? "unknown"
            : normalized;
    }

    private sealed class AttemptState
    {
        public int ConsecutiveFailures;
        public int AttemptsInWindow;
        public DateTimeOffset WindowStartUtc;
        public DateTimeOffset? BackoffUntilUtc;
        public DateTimeOffset? LockoutUntilUtc;
        public bool AlertThresholdRaised;
        public bool LockoutAlertRaised;
    }
}
