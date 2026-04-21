using Microsoft.AspNetCore.Authorization;
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
        var userId = context.User.GetUserId();
        if (!userId.HasValue)
        {
            context.Fail();
            return;
        }

        // Privileged roles always grant all permissions.
        if (context.User.IsInRole(RoleNameMapper.Admin) || context.User.IsInRole(RoleNameMapper.SuperAdmin))
        {
            context.Succeed(requirement);
            return;
        }

        if (await _permissionService.HasPermissionAsync(userId.Value, requirement.Permission))
            context.Succeed(requirement);
        else
            context.Fail();
    }
}
