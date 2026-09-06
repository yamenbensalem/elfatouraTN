namespace Web_GestCom.Auth;

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
    /// True when the current principal is authenticated. Distinct from HasActiveContext: an
    /// anonymous HTTP request (e.g. the login POST, which queries Utilisateur before any
    /// principal exists) has an active context but is not authenticated. Query filters that must
    /// stay visible pre-login (Utilisateur, looked up by AuthentifierAsync) gate on this.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// True when there is an active context (HTTP request or explicit background scope).
    /// AppDbContext only enforces tenant filters when this is true.
    /// </summary>
    bool HasActiveContext { get; }
}
