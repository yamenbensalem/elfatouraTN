using Web_T4C_GestCom.Services;

namespace T4C_GestCom_Desktop.Session;

/// <summary>Desktop equivalent of Web_T4C_GestCom's TenantService — backed by UserSession instead of HttpContext claims.</summary>
public sealed class DesktopTenantService(UserSession session) : ITenantService
{
    public int?   CurrentCompanyId  => session.CompanyId;
    public int?   CurrentUserId     => session.UserId;
    public string CurrentUserLogin  => session.Login;
}
