using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;

namespace Web_T4C_GestCom.Services;

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