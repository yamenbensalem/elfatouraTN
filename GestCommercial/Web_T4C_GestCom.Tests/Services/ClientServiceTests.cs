using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;
using Web_T4C_GestCom.Tests.Helpers;
using Xunit;

namespace Web_T4C_GestCom.Tests.Services;

public class ClientServiceTests
{
    private static ClientService CreateService(out AppDbContext db)
    {
        db = DbContextFactory.Create();
        return new ClientService(db, new NoOpJournalActiviteService());
    }

    private static Client MakeClient(string code, string nom)
        => new() { CodeClient = code, NomClient = nom, CodeDevise = 1 };

    // ── Add ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_WithExplicitCode_PersistsClient()
    {
        var svc = CreateService(out var db);

        var code = await svc.AddAsync(MakeClient("CL00001", "Alpha SARL"));

        Assert.Equal("CL00001", code);
        Assert.Equal(1, await db.Clients.CountAsync());
    }

    [Fact]
    public async Task AddAsync_WithoutCode_AutoGeneratesFirstCode()
    {
        var svc = CreateService(out _);
        var client = new Client { CodeClient = "", NomClient = "Auto SARL", CodeDevise = 1 };

        var code = await svc.AddAsync(client);

        Assert.Equal("CL00001", code);
    }

    [Fact]
    public async Task AddAsync_SecondClientWithoutCode_IncrementsNumber()
    {
        var svc = CreateService(out _);
        await svc.AddAsync(MakeClient("CL00001", "First"));

        var code = await svc.AddAsync(new Client { CodeClient = "", NomClient = "Second", CodeDevise = 1 });

        Assert.Equal("CL00002", code);
    }

    // ── GetAll ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_NoSearch_ReturnsAllOrderedByNom()
    {
        var svc = CreateService(out _);
        await svc.AddAsync(MakeClient("CL00002", "Zara Co"));
        await svc.AddAsync(MakeClient("CL00001", "Alpha SARL"));

        var results = await svc.GetAllAsync();

        Assert.Equal(2, results.Count);
        Assert.Equal("Alpha SARL", results[0].NomClient);
        Assert.Equal("Zara Co", results[1].NomClient);
    }

    [Fact]
    public async Task GetAllAsync_SearchByNom_FiltersResults()
    {
        var svc = CreateService(out _);
        await svc.AddAsync(MakeClient("CL00001", "Alpha SARL"));
        await svc.AddAsync(MakeClient("CL00002", "Beta Corp"));

        var results = await svc.GetAllAsync("Alpha");

        Assert.Single(results);
        Assert.Equal("Alpha SARL", results[0].NomClient);
    }

    [Fact]
    public async Task GetAllAsync_SearchByCode_FiltersResults()
    {
        var svc = CreateService(out _);
        await svc.AddAsync(MakeClient("CL00001", "Alpha SARL"));
        await svc.AddAsync(MakeClient("CL00002", "Beta Corp"));

        var results = await svc.GetAllAsync("CL00002");

        Assert.Single(results);
        Assert.Equal("CL00002", results[0].CodeClient);
    }

    [Fact]
    public async Task GetAllAsync_SearchByTel_FiltersResults()
    {
        var svc = CreateService(out _);
        var c = MakeClient("CL00001", "Alpha SARL");
        c.Tel = "71234567";
        await svc.AddAsync(c);
        await svc.AddAsync(MakeClient("CL00002", "Beta Corp"));

        var results = await svc.GetAllAsync("71234567");

        Assert.Single(results);
        Assert.Equal("CL00001", results[0].CodeClient);
    }

    [Fact]
    public async Task GetAllAsync_EmptySearch_ReturnsAll()
    {
        var svc = CreateService(out _);
        await svc.AddAsync(MakeClient("CL00001", "Alpha SARL"));
        await svc.AddAsync(MakeClient("CL00002", "Beta Corp"));

        var results = await svc.GetAllAsync("   ");

        Assert.Equal(2, results.Count);
    }

    // ── GetByCode ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByCodeAsync_ExistingCode_ReturnsClient()
    {
        var svc = CreateService(out _);
        await svc.AddAsync(MakeClient("CL00001", "Alpha SARL"));

        var result = await svc.GetByCodeAsync("CL00001");

        Assert.NotNull(result);
        Assert.Equal("Alpha SARL", result.NomClient);
    }

    [Fact]
    public async Task GetByCodeAsync_UnknownCode_ReturnsNull()
    {
        var svc = CreateService(out _);
        Assert.Null(await svc.GetByCodeAsync("UNKNOWN"));
    }

    // ── Update ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ChangesNomClient()
    {
        var svc = CreateService(out var db);
        await svc.AddAsync(MakeClient("CL00001", "Old Name"));

        var client = await db.Clients.FindAsync("CL00001");
        client!.NomClient = "New Name";
        await svc.UpdateAsync(client);

        Assert.Equal("New Name", (await db.Clients.FindAsync("CL00001"))!.NomClient);
    }

    [Fact]
    public async Task UpdateAsync_AfterFormLoadsReferenceListsAndGetByCode_DoesNotThrowIdentityConflict()
    {
        // Reproduces ClientForm.razor's OnInitializedAsync: it loads the Devise dropdown (tracked,
        // no AsNoTracking) and THEN GetByCodeAsync (AsNoTracking + Include(Devise)) in the same
        // DbContext scope — Update() previously tried to re-attach the AsNoTracking graph's Devise
        // instance, colliding with the already-tracked dropdown-list instance.
        var svc = CreateService(out var db);
        await svc.AddAsync(MakeClient("CL00001", "Old Name"));
        db.ChangeTracker.Clear(); // simulates a fresh circuit that never touched this row before

        await db.Devises.ToListAsync(); // simulates the form's currency dropdown load (tracked)
        var client = await svc.GetByCodeAsync("CL00001"); // AsNoTracking + Include(Devise)
        client!.NomClient = "New Name";

        var ex = await Record.ExceptionAsync(() => svc.UpdateAsync(client));

        Assert.Null(ex);
        Assert.Equal("New Name", (await db.Clients.FindAsync("CL00001"))!.NomClient);
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingCode_RemovesClient()
    {
        var svc = CreateService(out var db);
        await svc.AddAsync(MakeClient("CL00001", "To Delete"));

        await svc.DeleteAsync("CL00001");

        Assert.Equal(0, await db.Clients.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_UnknownCode_DoesNotThrow()
    {
        var svc = CreateService(out _);

        var ex = await Record.ExceptionAsync(() => svc.DeleteAsync("UNKNOWN"));

        Assert.Null(ex);
    }

    // ── Exists ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExistsAsync_ExistingCode_ReturnsTrue()
    {
        var svc = CreateService(out _);
        await svc.AddAsync(MakeClient("CL00001", "Test"));

        Assert.True(await svc.ExistsAsync("CL00001"));
    }

    [Fact]
    public async Task ExistsAsync_UnknownCode_ReturnsFalse()
    {
        var svc = CreateService(out _);
        Assert.False(await svc.ExistsAsync("UNKNOWN"));
    }
}
