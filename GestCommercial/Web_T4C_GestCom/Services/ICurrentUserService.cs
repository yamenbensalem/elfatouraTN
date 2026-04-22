using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;

namespace Web_T4C_GestCom.Services;

/// <summary>
/// Service scopé qui maintient l'identité de l'utilisateur courant pour toute la durée du circuit Blazor.
/// S'initialise automatiquement depuis AuthenticationStateProvider et reste compatible
/// avec une synchronisation explicite depuis MainLayout via SetCurrentUser().
/// </summary>
public interface ICurrentUserService
{
    string Login { get; }
    string Role { get; }
    bool IsAdmin { get; }
    bool IsSuperAdmin { get; }
    bool IsAuthenticated { get; }
    Task EnsureInitializedAsync();
    void SetCurrentUser(string login, string role);
    void Clear();
}

public class CurrentUserService : ICurrentUserService, IDisposable
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Task _initializationTask;

    private string _login = string.Empty;
    private string _role = string.Empty;

    public CurrentUserService(AuthenticationStateProvider authStateProvider, IHttpContextAccessor httpContextAccessor)
    {
        _authStateProvider = authStateProvider;
        _httpContextAccessor = httpContextAccessor;

        _authStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        _initializationTask = InitializeAsync();
    }

    public string Login
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated == true)
                return principal.Identity.Name ?? _login;

            return _login;
        }
    }

    public string Role
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated == true)
            {
                var role = principal.FindFirst(ClaimTypes.Role)?.Value;
                if (!string.IsNullOrWhiteSpace(role))
                    return RoleNameMapper.NormalizeKnownRoleName(role);
            }

            return _role;
        }
    }

    public bool IsAdmin => Role == RoleNameMapper.Admin;
    public bool IsSuperAdmin => Role == RoleNameMapper.SuperAdmin;

    public bool IsAuthenticated
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated == true)
                return true;

            return !string.IsNullOrWhiteSpace(_login);
        }
    }

    public Task EnsureInitializedAsync() => _initializationTask;

    private async Task InitializeAsync()
    {
        var state = await _authStateProvider.GetAuthenticationStateAsync();
        ApplyAuthenticationState(state);
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> authStateTask)
    {
        _ = SyncAuthenticationStateAsync(authStateTask);
    }

    private async Task SyncAuthenticationStateAsync(Task<AuthenticationState> authStateTask)
    {
        var state = await authStateTask;
        ApplyAuthenticationState(state);
    }

    private void ApplyAuthenticationState(AuthenticationState state)
    {
        if (state.User.Identity?.IsAuthenticated == true)
        {
            var login = state.User.Identity.Name ?? "utilisateur";
            var role = state.User.FindFirst(ClaimTypes.Role)?.Value ?? RoleNameMapper.Employe;
            SetCurrentUser(login, role);
            return;
        }

        Clear();
    }

    public void SetCurrentUser(string login, string role)
    {
        _login = string.IsNullOrWhiteSpace(login) ? string.Empty : login;
        _role = string.IsNullOrWhiteSpace(role)
            ? string.Empty
            : RoleNameMapper.NormalizeKnownRoleName(role);
    }

    public void Clear()
    {
        _login = string.Empty;
        _role = string.Empty;
    }

    public void Dispose()
        => _authStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
}
