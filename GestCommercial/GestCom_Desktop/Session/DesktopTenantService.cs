using Web_GestCom.Services;

namespace GestCom_Desktop.Session;

/// <summary>Desktop equivalent of Web_GestCom's TenantService — backed by UserSession instead of HttpContext claims.</summary>
public sealed class DesktopTenantService(UserSession session) : ITenantService
{
    public int?   CurrentCompanyId  => session.CompanyId;
    public int?   CurrentUserId     => session.UserId;
    public string CurrentUserLogin  => session.Login;
}
