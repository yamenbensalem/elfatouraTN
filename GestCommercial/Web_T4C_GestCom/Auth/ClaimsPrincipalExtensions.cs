using System.Security.Claims;

namespace Web_T4C_GestCom.Auth;

public static class ClaimsPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal? principal)
    {
        if (principal is null)
            return null;

        // Prefer NameIdentifier; keep legacy UserId claim for backward compatibility.
        var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? principal.FindFirstValue("UserId");

        return int.TryParse(raw, out var userId) ? userId : null;
    }

    public static int? GetCompanyId(this ClaimsPrincipal? principal)
    {
        if (principal is null)
            return null;

        var raw = principal.FindFirstValue("CompanyId");
        return int.TryParse(raw, out var companyId) ? companyId : null;
    }
}