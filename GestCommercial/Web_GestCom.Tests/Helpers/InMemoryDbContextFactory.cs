using Microsoft.EntityFrameworkCore;
using Web_GestCom.Data;

namespace Web_GestCom.Tests.Helpers;

/// <summary>
/// Fake IDbContextFactory<AppDbContext> for services built on the factory pattern
/// (PermissionService, FeatureFlagService). Reuses DbContextFactory's cached options so
/// every context it creates shares the same named in-memory store as the seeding context
/// returned by DbContextFactory.Create(dbName).
/// </summary>
public sealed class InMemoryDbContextFactory : IDbContextFactory<AppDbContext>
{
    private readonly DbContextOptions<AppDbContext> _opts;

    public InMemoryDbContextFactory(string dbName)
    {
        _opts = DbContextFactory.GetOrCreateOptions(dbName);
    }

    public AppDbContext CreateDbContext() => new(_opts);

    public Task<AppDbContext> CreateDbContextAsync(CancellationToken ct = default)
        => Task.FromResult(new AppDbContext(_opts));
}
