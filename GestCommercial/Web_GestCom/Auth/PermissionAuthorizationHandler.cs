using Microsoft.AspNetCore.Authorization;
using Web_GestCom.Services;

namespace Web_GestCom.Auth;

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

        // Admin always grants all business permissions within their own company. SuperAdmin does
        // NOT get this bypass — it's a platform-management role (companies, users, access rights)
        // with no business-data access by design, and only holds the superAdminModules
        // permissions (tenants/users-global/roles-global/journal-global) seeded in RolePermission.
        // Falling through to HasPermissionAsync below is what actually enforces that boundary.
        if (context.User.IsInRole(RoleNameMapper.Admin))
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
