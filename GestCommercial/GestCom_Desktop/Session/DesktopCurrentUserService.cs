using Web_GestCom.Services;

namespace GestCom_Desktop.Session;

/// <summary>Desktop equivalent of Web_GestCom's CurrentUserService — backed by UserSession instead of AuthenticationStateProvider/HttpContext.</summary>
public sealed class DesktopCurrentUserService(UserSession session) : ICurrentUserService
{
    public string Login           => session.Login;
    public string Role            => session.Role;
    public bool   IsAdmin         => Role == RoleNameMapper.Admin;
    public bool   IsSuperAdmin    => session.IsSuperAdmin || Role == RoleNameMapper.SuperAdmin;
    public bool   IsAuthenticated => session.IsAuthenticated;

    public Task EnsureInitializedAsync() => Task.CompletedTask;

    public void SetCurrentUser(string login, string role)
    {
        // Session is populated by LoginForm via UserSession.SignIn(); nothing to do here for the
        // desktop host, but the method must exist to satisfy ICurrentUserService.
    }

    public void Clear() => session.SignOut();
}
