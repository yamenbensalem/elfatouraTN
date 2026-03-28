using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Web_T4C_GestCom.Data;

namespace Web_T4C_GestCom.Services;

public interface IPermissionService
{
    Task<bool>                HasPermissionAsync(int userId, string permission);
    Task<IEnumerable<string>> GetUserPermissionsAsync(int userId);
    void                      InvalidateUser(int userId);
}

public class PermissionService : IPermissionService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public PermissionService(IDbContextFactory<AppDbContext> dbFactory, IMemoryCache cache)
    {
        _dbFactory = dbFactory;
        _cache     = cache;
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(int userId)
    {
        var key = $"perms:{userId}";
        if (_cache.TryGetValue(key, out IEnumerable<string>? cached) && cached is not null)
            return cached;

        await using var db = await _dbFactory.CreateDbContextAsync();

        var perms = await db.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Select(rp => rp.Permission!.Feature + "." + rp.Permission.Action)
            .Distinct()
            .ToListAsync();

        _cache.Set(key, (IEnumerable<string>)perms, CacheTtl);
        return perms;
    }

    public async Task<bool> HasPermissionAsync(int userId, string permission)
    {
        var perms = await GetUserPermissionsAsync(userId);
        return perms.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    public void InvalidateUser(int userId) =>
        _cache.Remove($"perms:{userId}");
}
