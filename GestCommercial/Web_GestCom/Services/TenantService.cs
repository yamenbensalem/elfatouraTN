using Microsoft.AspNetCore.Http;
using Web_GestCom.Auth;
using Web_GestCom.Services;

namespace Web_GestCom.Services;

/// <summary>
/// L'interface <see cref="ITenantService"/> vient de Web_GestCom.Core (partagée avec
/// GestCom_Desktop, qui fournit sa propre implémentation liée à la session connectée) — seule
/// cette implémentation liée à IHttpContextAccessor reste ici.
/// </summary>
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
