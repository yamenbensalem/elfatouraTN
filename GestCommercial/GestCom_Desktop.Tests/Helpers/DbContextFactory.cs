using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Web_GestCom.Auth;
using Web_GestCom.Data;

namespace GestCom_Desktop.Tests.Helpers;

/// <summary>
/// Produces isolated in-memory AppDbContext instances for desktop unit tests, driven by
/// IExecutionContext instead of HttpExecutionContext — mirrors Web_GestCom.Tests'
/// DbContextFactory (options cached by db name, so a seed context and a differently-scoped
/// tenant context can share the same in-memory store within one test).
/// </summary>
public static class DbContextFactory
{
    private static readonly Dictionary<string, DbContextOptions<AppDbContext>> _optionsCache = [];
    private static readonly object _lock = new();

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

    public static AppDbContext Create(IExecutionContext? executionContext = null, string? dbName = null)
    {
        var options = GetOrCreateOptions(dbName ?? Guid.NewGuid().ToString());
        var ctx = new AppDbContext(options, executionContext);
        ctx.Database.EnsureCreated();
        return ctx;
    }
}
