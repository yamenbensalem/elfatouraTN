namespace GestCom_Desktop.Session;

/// <summary>
/// Single in-memory holder for "who is logged in" during the lifetime of the desktop process.
/// Registered as a singleton; populated once by LoginForm after a successful
/// IUtilisateurService.AuthentifierAsync(), then read by DesktopExecutionContext /
/// DesktopCurrentUserService / DesktopTenantService — the desktop-side equivalents of the
/// HTTP-backed implementations used by the Blazor app (HttpExecutionContext, CurrentUserService,
/// TenantService).
/// </summary>
public sealed class UserSession
{
    public int?   UserId       { get; private set; }
    public int?   CompanyId    { get; private set; }
    public string Login        { get; private set; } = string.Empty;
    public string Role         { get; private set; } = string.Empty;
    public bool   IsSuperAdmin { get; private set; }
    public bool   IsAuthenticated => UserId is not null;

    public void SignIn(int userId, int? companyId, string login, string role, bool isSuperAdmin)
    {
        UserId = userId;
        CompanyId = companyId;
        Login = login;
        Role = role;
        IsSuperAdmin = isSuperAdmin;
    }

    public void SignOut()
    {
        UserId = null;
        CompanyId = null;
        Login = string.Empty;
        Role = string.Empty;
        IsSuperAdmin = false;
    }
}
