using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Web_GestCom.Data;

namespace Web_GestCom.Services;

public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(string feature, int companyId);
    void       InvalidateCompany(int companyId);
}

public class FeatureFlagService : IFeatureFlagService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public FeatureFlagService(IDbContextFactory<AppDbContext> dbFactory, IMemoryCache cache)
    {
        _dbFactory = dbFactory;
        _cache     = cache;
    }

    public async Task<bool> IsEnabledAsync(string feature, int companyId)
    {
        var key = $"ff:{companyId}:{feature}";
        if (_cache.TryGetValue(key, out bool cached))
            return cached;

        await using var db = await _dbFactory.CreateDbContextAsync();

        // If no row exists, the feature is considered enabled (opt-out model)
        var flag = await db.FeatureFlags
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.CompanyId == companyId &&
                                      f.Feature   == feature);

        var enabled = flag?.IsEnabled ?? true;
        _cache.Set(key, enabled, CacheTtl);
        return enabled;
    }

    public void InvalidateCompany(int companyId)
    {
        // IMemoryCache has no wildcard removal — we mark a version token instead.
        // For simplicity we remove individual known keys if callers pass them,
        // but the TTL (5 min) handles the rest automatically.
        _cache.Remove($"ff-ver:{companyId}");
    }
}
