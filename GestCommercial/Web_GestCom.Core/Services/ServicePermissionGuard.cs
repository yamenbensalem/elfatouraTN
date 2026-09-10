using Microsoft.EntityFrameworkCore;
using Web_GestCom.Data;

namespace Web_GestCom.Services;

internal static class ServicePermissionGuard
{
    public static async Task EnsureAsync(
        AppDbContext db,
        ICurrentUserService? currentUser,
        IPermissionService? permissionService,
        string permission)
    {
        // Keep unit tests/backward-compatible call sites working when auth services are not wired.
        if (currentUser is null || permissionService is null)
            return;

        await currentUser.EnsureInitializedAsync();

        // Admin bypasses business-permission checks within their own company. SuperAdmin does
        // NOT — it's a platform-management role with no business-data write access by design
        // (see PermissionAuthorizationHandler for the matching read-side boundary). A SuperAdmin
        // falls through to the HasPermissionAsync check below like any other unprivileged role,
        // and fails it since SuperAdmin only holds the platform-scoped permissions.
        if (currentUser.IsAdmin)
            return;

        if (!currentUser.IsAuthenticated || string.IsNullOrWhiteSpace(currentUser.Login))
            throw new UnauthorizedAccessException($"Acces refuse: permission '{permission}' requise.");

        var userId = await db.Utilisateurs
            .AsNoTracking()
            .Where(u => u.Login == currentUser.Login)
            .Select(u => (int?)u.Id)
            .FirstOrDefaultAsync();

        if (!userId.HasValue || !await permissionService.HasPermissionAsync(userId.Value, permission))
            throw new UnauthorizedAccessException($"Acces refuse: permission '{permission}' requise.");
    }
}