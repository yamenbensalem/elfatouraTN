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
            .Select(u => new { u.CompanyId, u.Role, u.IsSuperAdmin })
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

        var perms = await db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Where(ur => ur.Role != null &&
                         (ur.Role.CompanyId == null ||
                          (effectiveCompanyId.HasValue && ur.Role.CompanyId == effectiveCompanyId)))
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Select(rp => rp.Permission!.Feature + "." + rp.Permission.Action)
            .Distinct()
            .ToListAsync();

        if (perms.Count == 0)
        {
            var legacyRole = await db.Utilisateurs
                .Where(u => u.Id == userId)
                .Select(u => u.Role)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(legacyRole))
            {
                var normalizedRole = userMetadata.IsSuperAdmin
                    ? RoleNameMapper.SuperAdmin
                    : RoleNameMapper.NormalizeKnownRoleName(legacyRole);

                perms = await db.AppRoles
                    .Where(r => r.Name == normalizedRole)
                    .Where(r => r.CompanyId == null ||
                                (effectiveCompanyId.HasValue && r.CompanyId == effectiveCompanyId))
                    .SelectMany(r => r.RolePermissions)
                    .Select(rp => rp.Permission!.Feature + "." + rp.Permission.Action)
                    .Distinct()
                    .ToListAsync();
            }
        }

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
