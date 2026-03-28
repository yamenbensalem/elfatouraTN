using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Web_T4C_GestCom.Services;

namespace Web_T4C_GestCom.Auth;

public class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;

    public PermissionAuthorizationHandler(IPermissionService permissionService)
        => _permissionService = permissionService;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
        {
            context.Fail();
            return;
        }

        // Admin legacy role always grants all permissions
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        if (await _permissionService.HasPermissionAsync(userId, requirement.Permission))
            context.Succeed(requirement);
        else
            context.Fail();
    }
}
