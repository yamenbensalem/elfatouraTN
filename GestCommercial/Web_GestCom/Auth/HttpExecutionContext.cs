using Microsoft.AspNetCore.Http;

namespace Web_GestCom.Auth;

/// <summary>
/// Resolves execution context from the active HTTP request via IHttpContextAccessor.
/// Registered as Scoped so it follows the request lifetime.
/// </summary>
public sealed class HttpExecutionContext : IExecutionContext
{
    private readonly IHttpContextAccessor _accessor;

    public HttpExecutionContext(IHttpContextAccessor accessor)
        => _accessor = accessor;

    public int?  CurrentCompanyId => _accessor.HttpContext?.User.GetCompanyId();
    public bool  IsSuperAdmin     => _accessor.HttpContext?.User.IsSuperAdmin() == true;
    public bool  HasActiveContext => _accessor.HttpContext is not null;
    public bool  IsAuthenticated  => _accessor.HttpContext?.User.Identity?.IsAuthenticated == true;
}
