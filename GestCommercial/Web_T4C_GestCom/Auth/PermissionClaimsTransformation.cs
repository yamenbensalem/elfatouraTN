using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Web_T4C_GestCom.Services;

namespace Web_T4C_GestCom.Auth;

/// <summary>
/// Injecte les claims de permissions dans l'identité de l'utilisateur au début de chaque requête.
/// Permet d'utiliser User.HasClaim("Permission", "factures.view") dans les composants Blazor.
/// </summary>
public class PermissionClaimsTransformation : IClaimsTransformation
{
    private const string PermissionClaimType = "Permission";
    private readonly IPermissionService _permissionService;

    public PermissionClaimsTransformation(IPermissionService permissionService)
        => _permissionService = permissionService;

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Already transformed this request — avoid double-adding
        if (principal.HasClaim(c => c.Type == PermissionClaimType))
            return principal;

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return principal;

        var permissions = await _permissionService.GetUserPermissionsAsync(userId);

        var clone = principal.Clone();
        var identity = (ClaimsIdentity)clone.Identity!;
        foreach (var perm in permissions)
            identity.AddClaim(new Claim(PermissionClaimType, perm));

        return clone;
    }
}
