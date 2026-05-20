using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Web_T4C_GestCom.Data;

namespace Web_T4C_GestCom.Tests.Helpers;

/// <summary>
/// Produces isolated in-memory AppDbContext instances for unit tests.
/// Each call uses a unique database name so tests cannot interfere with each other.
/// EnsureCreated() applies HasData() seed rows (Devise, ModePayement, TvaProduit, etc.).
/// TransactionIgnoredWarning is suppressed — InMemory silently ignores transactions.
///
/// Options are cached by database name so that multiple contexts built for the SAME named
/// in-memory database always share the exact same DbContextOptions instance, which guarantees
/// EF Core routes them to the same in-memory store.
/// </summary>
public static class DbContextFactory
{
    private static readonly Dictionary<string, DbContextOptions<AppDbContext>> _optionsCache = [];
    private static readonly object _lock = new();

    /// <summary>
    /// Returns (and caches) the DbContextOptions for a given in-memory database name.
    /// Call this from custom IDbContextFactory implementations so they share the store
    /// with the seeder context returned by <see cref="Create"/>.
    /// </summary>
    public static DbContextOptions<AppDbContext> GetOrCreateOptions(string dbName)
    {
        lock (_lock)
        {
            if (!_optionsCache.TryGetValue(dbName, out var opts))
            {
                opts = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(dbName)
                    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .Options;
                _optionsCache[dbName] = opts;
            }
            return opts;
        }
    }

    public static AppDbContext Create(string? dbName = null)
    {
        var name = dbName ?? Guid.NewGuid().ToString();
        var options = GetOrCreateOptions(name);
        var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }
}
