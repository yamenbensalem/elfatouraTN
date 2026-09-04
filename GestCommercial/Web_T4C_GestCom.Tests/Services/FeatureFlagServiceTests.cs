using Microsoft.Extensions.Caching.Memory;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;
using Web_T4C_GestCom.Tests.Helpers;
using Xunit;

namespace Web_T4C_GestCom.Tests.Services;

public class FeatureFlagServiceTests
{
    private static (FeatureFlagService svc, InMemoryDbContextFactory factory) CreateSvc(string? dbName = null)
    {
        var name = dbName ?? Guid.NewGuid().ToString();
        var factory = new InMemoryDbContextFactory(name);
        var svc = new FeatureFlagService(factory, new MemoryCache(new MemoryCacheOptions()));
        return (svc, factory);
    }

    [Fact]
    public async Task IsEnabledAsync_NoFlagRow_DefaultsToEnabled()
    {
        var (svc, _) = CreateSvc();

        var enabled = await svc.IsEnabledAsync("some-feature", companyId: 1);

        Assert.True(enabled);
    }

    [Fact]
    public async Task IsEnabledAsync_FlagDisabledInDb_ReturnsFalse()
    {
        var (svc, factory) = CreateSvc();
        await using (var db = factory.CreateDbContext())
        {
            db.FeatureFlags.Add(new FeatureFlag { CompanyId = 1, Feature = "beta-invoices", IsEnabled = false });
            await db.SaveChangesAsync();
        }

        var enabled = await svc.IsEnabledAsync("beta-invoices", companyId: 1);

        Assert.False(enabled);
    }

    [Fact]
    public async Task IsEnabledAsync_FlagScopedToDifferentCompany_DoesNotApply()
    {
        var (svc, factory) = CreateSvc();
        await using (var db = factory.CreateDbContext())
        {
            db.FeatureFlags.Add(new FeatureFlag { CompanyId = 1, Feature = "beta-invoices", IsEnabled = false });
            await db.SaveChangesAsync();
        }

        var enabled = await svc.IsEnabledAsync("beta-invoices", companyId: 2);

        Assert.True(enabled);
    }

    [Fact]
    public async Task IsEnabledAsync_SecondCall_IsServedFromCache()
    {
        var (svc, factory) = CreateSvc();
        await using (var db = factory.CreateDbContext())
        {
            db.FeatureFlags.Add(new FeatureFlag { CompanyId = 1, Feature = "beta-invoices", IsEnabled = false });
            await db.SaveChangesAsync();
        }

        var first = await svc.IsEnabledAsync("beta-invoices", companyId: 1);

        // Flip the flag directly in the DB — the cached value should still be served.
        await using (var db = factory.CreateDbContext())
        {
            var flag = db.FeatureFlags.First(f => f.CompanyId == 1 && f.Feature == "beta-invoices");
            flag.IsEnabled = true;
            await db.SaveChangesAsync();
        }

        var second = await svc.IsEnabledAsync("beta-invoices", companyId: 1);

        Assert.False(first);
        Assert.False(second); // still cached
    }
}
