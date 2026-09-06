using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Web_GestCom.Data;

namespace Web_GestCom.Services;

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
    private readonly ITenantService? _tenantService;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public PermissionService(
        IDbContextFactory<AppDbContext> dbFactory,
        IMemoryCache cache,
        ITenantService? tenantService = null)
    {
        _dbFactory = dbFactory;
        _cache     = cache;
        _tenantService = tenantService;
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(int userId)
    {
        var currentCompanyId = _tenantService?.CurrentCompanyId;
        var scope = currentCompanyId?.ToString() ?? "n/a";
        var versionToken = _cache.Get<string>($"perms-ver:{userId}") ?? "0";
        var key = $"perms:{userId}:{scope}:{versionToken}";
        if (_cache.TryGetValue(key, out IEnumerable<string>? cached) && cached is not null)
            return cached;

        await using var db = await _dbFactory.CreateDbContextAsync();

        var userMetadata = await db.Utilisateurs
            .AsNoTracking()
            .Where(u => u.Id == userId && u.Actif)
            .Select(u => new { u.CompanyId, u.IsSuperAdmin })
            .FirstOrDefaultAsync();

        if (userMetadata is null)
            return Array.Empty<string>();

        var effectiveCompanyId = currentCompanyId ?? userMetadata.CompanyId;
        var enforceTenantContext = _tenantService is not null;

        if (!userMetadata.IsSuperAdmin && enforceTenantContext)
        {
            if (!effectiveCompanyId.HasValue)
                return Array.Empty<string>();

            if (userMetadata.CompanyId != effectiveCompanyId)
                return Array.Empty<string>();
        }

        var perms = await (
            from ur in db.UserRoles
            join r  in db.AppRoles       on ur.RoleId        equals r.Id
            join rp in db.RolePermissions on r.Id             equals rp.RoleId
            join p  in db.Permissions    on rp.PermissionId  equals p.Id
            where ur.UserId == userId
            where r.CompanyId == null ||
                  !effectiveCompanyId.HasValue ||
                  r.CompanyId == effectiveCompanyId
            select p.Feature + "." + p.Action
        ).Distinct().ToListAsync();

        _cache.Set(key, (IEnumerable<string>)perms, CacheTtl);
        return perms;
    }

    public async Task<bool> HasPermissionAsync(int userId, string permission)
    {
        var perms = await GetUserPermissionsAsync(userId);
        return perms.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    public void InvalidateUser(int userId)
    {
        _cache.Set($"perms-ver:{userId}", Guid.NewGuid().ToString("N"), CacheTtl);
    }
}
