using Microsoft.AspNetCore.Http;
using Web_T4C_GestCom.Auth;
using Web_T4C_GestCom.Services;

namespace Web_T4C_GestCom.Services;

public interface ITenantService
{
    int?   CurrentCompanyId { get; }
    int?   CurrentUserId    { get; }
    string CurrentUserLogin { get; }
}

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _http;

    public TenantService(IHttpContextAccessor http) => _http = http;

    private System.Security.Claims.ClaimsPrincipal? User =>
        _http.HttpContext?.User;

    public int? CurrentCompanyId
    {
        get
        {
            return User.GetCompanyId();
        }
    }

    public int? CurrentUserId
    {
        get
        {
            return User.GetUserId();
        }
    }

    public string CurrentUserLogin =>
        User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? string.Empty;
}
