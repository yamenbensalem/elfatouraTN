using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;

namespace Web_T4C_GestCom.Tests.Helpers;

/// <summary>
/// Produces isolated in-memory AppDbContext instances for unit tests.
/// Each call uses a unique database name so tests cannot interfere with each other.
/// EnsureCreated() applies HasData() seed rows (Devise, ModePayement, TvaProduit, etc.).
/// </summary>
public static class DbContextFactory
{
    public static AppDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }
}
