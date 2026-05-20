namespace Web_T4C_GestCom.Auth;

/// <summary>
/// Explicit execution context for background tasks and scheduled jobs.
/// Caller supplies the tenant and superadmin flag — no HTTP request required.
/// </summary>
public sealed class BackgroundExecutionContext : IExecutionContext
{
    public BackgroundExecutionContext(int? companyId, bool isSuperAdmin = false)
    {
        CurrentCompanyId = companyId;
        IsSuperAdmin     = isSuperAdmin;
    }

    public int?  CurrentCompanyId { get; }
    public bool  IsSuperAdmin     { get; }
    public bool  HasActiveContext => true;
}
