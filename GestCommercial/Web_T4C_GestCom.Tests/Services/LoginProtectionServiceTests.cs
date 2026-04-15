using Microsoft.Extensions.Options;
using Web_T4C_GestCom.Services;
using Xunit;

namespace Web_T4C_GestCom.Tests.Services;

public class LoginProtectionServiceTests
{
    private static LoginProtectionService CreateService(LoginProtectionOptions? options = null)
    {
        options ??= new LoginProtectionOptions();
        return new LoginProtectionService(Options.Create(options));
    }

    [Fact]
    public void RegisterFailure_TriggersProgressiveBackoff()
    {
        var svc = CreateService(new LoginProtectionOptions
        {
            ProgressiveBackoffStartFailures = 2,
            BackoffBaseSeconds = 2,
            BackoffMaxSeconds = 10,
            LoginLockoutThreshold = 50,
            IpLockoutThreshold = 50,
            MaxFailuresPerLoginWindow = 100,
            MaxFailuresPerIpWindow = 100
        });

        var now = new DateTimeOffset(2026, 4, 15, 12, 0, 0, TimeSpan.Zero);
        _ = svc.RegisterFailure("alice", "10.0.0.1", now);
        var second = svc.RegisterFailure("alice", "10.0.0.1", now);

        Assert.True(second.IsBlocked);
        Assert.NotNull(second.RetryAfterUtc);
        Assert.True(second.RetryAfterUtc!.Value >= now.AddSeconds(2));
    }

    [Fact]
    public void RegisterFailure_TriggersTemporaryLockout_PerLogin()
    {
        var svc = CreateService(new LoginProtectionOptions
        {
            LoginLockoutThreshold = 3,
            LoginLockoutSeconds = 300,
            IpLockoutThreshold = 50,
            MaxFailuresPerLoginWindow = 100,
            MaxFailuresPerIpWindow = 100,
            ProgressiveBackoffStartFailures = 99
        });

        var now = new DateTimeOffset(2026, 4, 15, 12, 0, 0, TimeSpan.Zero);
        _ = svc.RegisterFailure("bob", "10.0.0.2", now);
        _ = svc.RegisterFailure("bob", "10.0.0.2", now.AddSeconds(1));
        var third = svc.RegisterFailure("bob", "10.0.0.2", now.AddSeconds(2));

        Assert.True(third.IsBlocked);
        Assert.Equal("login", third.ReasonCode);
        Assert.NotNull(third.RetryAfterUtc);
        Assert.True(third.RetryAfterUtc!.Value >= now.AddSeconds(2 + 300));
        Assert.Contains("login.lockout", third.Alerts);
    }

    [Fact]
    public void RegisterFailure_TriggersTemporaryLockout_PerIp_AcrossMultipleLogins()
    {
        var svc = CreateService(new LoginProtectionOptions
        {
            MaxFailuresPerIpWindow = 3,
            IpWindowSeconds = 600,
            IpLockoutSeconds = 180,
            LoginLockoutThreshold = 99,
            MaxFailuresPerLoginWindow = 99,
            ProgressiveBackoffStartFailures = 99
        });

        var now = new DateTimeOffset(2026, 4, 15, 12, 0, 0, TimeSpan.Zero);
        _ = svc.RegisterFailure("user1", "10.0.0.3", now);
        _ = svc.RegisterFailure("user2", "10.0.0.3", now.AddSeconds(1));
        var third = svc.RegisterFailure("user3", "10.0.0.3", now.AddSeconds(2));

        Assert.True(third.IsBlocked);
        Assert.Equal("ip", third.ReasonCode);
        Assert.Contains("ip.lockout", third.Alerts);

        var followUp = svc.Evaluate("user4", "10.0.0.3", now.AddSeconds(3));
        Assert.True(followUp.IsBlocked);
        Assert.Equal("ip", followUp.ReasonCode);
    }

    [Fact]
    public void RegisterSuccess_ClearsLoginSpecificBlockingState()
    {
        var svc = CreateService(new LoginProtectionOptions
        {
            ProgressiveBackoffStartFailures = 2,
            BackoffBaseSeconds = 5,
            BackoffMaxSeconds = 60,
            LoginLockoutThreshold = 50,
            IpLockoutThreshold = 50,
            MaxFailuresPerLoginWindow = 100,
            MaxFailuresPerIpWindow = 100
        });

        var now = new DateTimeOffset(2026, 4, 15, 12, 0, 0, TimeSpan.Zero);
        _ = svc.RegisterFailure("carol", "10.0.0.4", now);
        _ = svc.RegisterFailure("carol", "10.0.0.4", now);

        var blocked = svc.Evaluate("carol", "10.0.0.4", now);
        Assert.True(blocked.IsBlocked);

        svc.RegisterSuccess("carol", "10.0.0.4");

        var afterSuccess = svc.Evaluate("carol", "10.0.0.4", now);
        Assert.False(afterSuccess.IsBlocked);
    }
}
