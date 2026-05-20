namespace Web_T4C_GestCom.Auth;

/// <summary>
/// Abstracts the ambient tenant / security context so that AppDbContext can enforce tenant
/// isolation in both HTTP request scope (HttpExecutionContext) and background tasks
/// (BackgroundExecutionContext).  Null injection = no active context = filters disabled.
/// </summary>
public interface IExecutionContext
{
    /// <summary>ID of the current tenant. Null means no active tenant scope.</summary>
    int? CurrentCompanyId { get; }

    /// <summary>True when the current principal has SuperAdmin privileges.</summary>
    bool IsSuperAdmin { get; }

    /// <summary>
    /// True when there is an active context (HTTP request or explicit background scope).
    /// AppDbContext only enforces tenant filters when this is true.
    /// </summary>
    bool HasActiveContext { get; }
}
