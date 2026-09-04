using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;
using Web_T4C_GestCom.Tests.Helpers;
using Xunit;

namespace Web_T4C_GestCom.Tests.Services;

public class JournalActiviteServiceTests
{
    private static (JournalActiviteService svc, AppDbContext db) CreateService(string login = "admin")
    {
        var db = DbContextFactory.Create();
        return (new JournalActiviteService(db, new StubCurrentUserService(login)), db);
    }

    [Fact]
    public async Task EnregistrerAsync_PersistsEntryWithLoginActionAndEntite()
    {
        var (svc, db) = CreateService("jdoe");

        await svc.EnregistrerAsync("Ajout", "Client", "CL00001", "détail");

        var entries = await svc.GetAllAsync();
        Assert.Single(entries);
        Assert.Equal("jdoe", entries[0].Login);
        Assert.Equal("Ajout", entries[0].Action);
        Assert.Equal("Client", entries[0].Entite);
        Assert.Equal("CL00001", entries[0].CodeEntite);
        Assert.Equal("détail", entries[0].Detail);
    }

    [Fact]
    public async Task EnregistrerAsync_SwallowsExceptions_NeverThrows()
    {
        var db = DbContextFactory.Create();
        await db.DisposeAsync(); // force SaveChangesAsync to fail on a disposed context
        var svc = new JournalActiviteService(db, new StubCurrentUserService("admin"));

        var ex = await Record.ExceptionAsync(() => svc.EnregistrerAsync("Ajout", "Client"));

        Assert.Null(ex);
    }

    [Fact]
    public async Task GetAllAsync_FilterByLogin_ReturnsOnlyMatchingEntries()
    {
        var db = DbContextFactory.Create();
        await new JournalActiviteService(db, new StubCurrentUserService("alice")).EnregistrerAsync("Ajout", "Client");
        await new JournalActiviteService(db, new StubCurrentUserService("bob")).EnregistrerAsync("Ajout", "Client");
        var svc = new JournalActiviteService(db, new StubCurrentUserService("alice"));

        var entries = await svc.GetAllAsync(login: "alice");

        Assert.Single(entries);
        Assert.Equal("alice", entries[0].Login);
    }

    [Fact]
    public async Task GetAllAsync_FilterByEntite_ReturnsOnlyMatchingEntries()
    {
        var (svc, _) = CreateService();
        await svc.EnregistrerAsync("Ajout", "Client");
        await svc.EnregistrerAsync("Ajout", "Produit");

        var entries = await svc.GetAllAsync(entite: "Produit");

        Assert.Single(entries);
        Assert.Equal("Produit", entries[0].Entite);
    }

    [Fact]
    public async Task GetAllAsync_FilterByDateRange_ExcludesOutOfRangeEntries()
    {
        var (svc, db) = CreateService();
        await svc.EnregistrerAsync("Ajout", "Client");

        var futureEntries = await svc.GetAllAsync(debut: DateTime.Today.AddDays(1));

        Assert.Empty(futureEntries);
    }

    [Fact]
    public async Task GetLoginsDistinctsAsync_ReturnsUniqueSortedLogins()
    {
        var db = DbContextFactory.Create();
        await new JournalActiviteService(db, new StubCurrentUserService("bob")).EnregistrerAsync("Ajout", "Client");
        await new JournalActiviteService(db, new StubCurrentUserService("alice")).EnregistrerAsync("Ajout", "Client");
        await new JournalActiviteService(db, new StubCurrentUserService("alice")).EnregistrerAsync("Modification", "Client");
        var svc = new JournalActiviteService(db, new StubCurrentUserService("alice"));

        var logins = await svc.GetLoginsDistinctsAsync();

        Assert.Equal(["alice", "bob"], logins);
    }

    [Fact]
    public async Task GetEntitesDistinctesAsync_ReturnsUniqueSortedEntites()
    {
        var (svc, _) = CreateService();
        await svc.EnregistrerAsync("Ajout", "Produit");
        await svc.EnregistrerAsync("Ajout", "Client");
        await svc.EnregistrerAsync("Modification", "Client");

        var entites = await svc.GetEntitesDistinctesAsync();

        Assert.Equal(["Client", "Produit"], entites);
    }

    // ── Purge ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PurgeAsync_RemovesOnlyEntriesOlderThanThreshold()
    {
        var (svc, db) = CreateService();
        db.JournalActivites.Add(new JournalActivite { Login = "admin", Action = "Ajout", Entite = "Client", DateHeure = DateTime.Now.AddMonths(-13) });
        db.JournalActivites.Add(new JournalActivite { Login = "admin", Action = "Ajout", Entite = "Client", DateHeure = DateTime.Now.AddMonths(-1) });
        await db.SaveChangesAsync();

        var deleted = await svc.PurgeAsync(12);

        Assert.Equal(1, deleted);
        var remaining = await db.JournalActivites.Where(j => j.Action == "Ajout").ToListAsync();
        Assert.Single(remaining);
    }

    [Fact]
    public async Task PurgeAsync_RecordsPurgeJournalEntry()
    {
        var (svc, db) = CreateService();
        db.JournalActivites.Add(new JournalActivite { Login = "admin", Action = "Ajout", Entite = "Client", DateHeure = DateTime.Now.AddMonths(-13) });
        await db.SaveChangesAsync();

        await svc.PurgeAsync(12);

        var purgeEntries = await db.JournalActivites.Where(j => j.Action == "Purge").ToListAsync();
        Assert.Single(purgeEntries);
        Assert.Contains("1 entrée", purgeEntries[0].Detail);
    }

    [Fact]
    public async Task PurgeAsync_NoOldEntries_ReturnsZeroAndWritesNoPurgeEntry()
    {
        var (svc, db) = CreateService();
        db.JournalActivites.Add(new JournalActivite { Login = "admin", Action = "Ajout", Entite = "Client", DateHeure = DateTime.Now.AddDays(-1) });
        await db.SaveChangesAsync();

        var deleted = await svc.PurgeAsync(12);

        Assert.Equal(0, deleted);
        Assert.Equal(0, await db.JournalActivites.CountAsync(j => j.Action == "Purge"));
    }

    [Fact]
    public async Task PurgeAsync_ZeroOrNegativeMonths_Throws()
    {
        var (svc, _) = CreateService();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => svc.PurgeAsync(0));
    }
}
