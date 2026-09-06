using Web_GestCom.Auth;

namespace GestCom_Desktop.Session;

/// <summary>Desktop equivalent of HttpExecutionContext — backed by UserSession instead of HttpContext.</summary>
public sealed class DesktopExecutionContext(UserSession session) : IExecutionContext
{
    public int?  CurrentCompanyId  => session.CompanyId;
    public bool  IsSuperAdmin      => session.IsSuperAdmin;
    public bool  HasActiveContext  => session.IsAuthenticated;
}
