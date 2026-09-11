using Microsoft.EntityFrameworkCore;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;
using Web_GestCom.Tests.Helpers;
using Xunit;

namespace Web_GestCom.Tests.Services;

public class FournisseurServiceTests
{
    private static FournisseurService CreateService(out AppDbContext db)
    {
        db = DbContextFactory.Create();
        return new FournisseurService(db, new NoOpJournalActiviteService());
    }

    private static Fournisseur MakeFournisseur(string code, string nom)
        => new() { CodeFournisseur = code, NomFournisseur = nom, CodeDevise = 1 };

    // ── Add ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_WithExplicitCode_PersistsFournisseur()
    {
        var svc = CreateService(out var db);

        var code = await svc.AddAsync(MakeFournisseur("FO00001", "Alpha Co"));

        Assert.Equal("FO00001", code);
        Assert.Equal(1, await db.Fournisseurs.CountAsync());
    }

    [Fact]
    public async Task AddAsync_WithoutCode_AutoGeneratesFirstCode()
    {
        var svc = CreateService(out _);
        var f = new Fournisseur { CodeFournisseur = "", NomFournisseur = "Auto Co", CodeDevise = 1 };

        var code = await svc.AddAsync(f);

        Assert.Equal("FO00001", code);
    }

    [Fact]
    public async Task AddAsync_SecondFournisseurWithoutCode_IncrementsNumber()
    {
        var svc = CreateService(out _);
        await svc.AddAsync(MakeFournisseur("FO00001", "First"));

        var code = await svc.AddAsync(new Fournisseur { CodeFournisseur = "", NomFournisseur = "Second", CodeDevise = 1 });

        Assert.Equal("FO00002", code);
    }

    [Fact]
    public async Task AddAsync_AfterDeletingAMiddleFournisseur_DoesNotCollideWithTheSurvivingHighestCode()
    {
        // Real production bug (see ProduitServiceTests for the exact reported error): code
        // generation used COUNT(*) + 1. With FO00001/02/03 present (count=3) and FO00002 deleted
        // (count=2), the next auto-generated code recomputed count+1 = "FO00003" — which still
        // existed — causing a primary-key violation. Must be based on the MAX existing number,
        // not the row count.
        var svc = CreateService(out _);
        await svc.AddAsync(MakeFournisseur("FO00001", "First"));
        await svc.AddAsync(MakeFournisseur("FO00002", "Second"));
        await svc.AddAsync(MakeFournisseur("FO00003", "Third"));
        await svc.DeleteAsync("FO00002");

        var code = await svc.AddAsync(new Fournisseur { CodeFournisseur = "", NomFournisseur = "Fourth", CodeDevise = 1 });

        Assert.Equal("FO00004", code);
    }

    // ── GetAll ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_NoSearch_ReturnsAllOrderedByNom()
    {
        var svc = CreateService(out _);
        await svc.AddAsync(MakeFournisseur("FO00002", "Zara Industries"));
        await svc.AddAsync(MakeFournisseur("FO00001", "Alpha Supplies"));

        var results = await svc.GetAllAsync();

        Assert.Equal(2, results.Count);
        Assert.Equal("Alpha Supplies", results[0].NomFournisseur);
        Assert.Equal("Zara Industries", results[1].NomFournisseur);
    }

    [Fact]
    public async Task GetAllAsync_SearchByNom_FiltersResults()
    {
        var svc = CreateService(out _);
        await svc.AddAsync(MakeFournisseur("FO00001", "TechParts"));
        await svc.AddAsync(MakeFournisseur("FO00002", "Office Supply"));

        var results = await svc.GetAllAsync("TechParts");

        Assert.Single(results);
        Assert.Equal("TechParts", results[0].NomFournisseur);
    }

    [Fact]
    public async Task GetAllAsync_SearchByCode_FiltersResults()
    {
        var svc = CreateService(out _);
        await svc.AddAsync(MakeFournisseur("FO00001", "Alpha"));
        await svc.AddAsync(MakeFournisseur("FO00002", "Beta"));

        var results = await svc.GetAllAsync("FO00001");

        Assert.Single(results);
        Assert.Equal("FO00001", results[0].CodeFournisseur);
    }

    [Fact]
    public async Task GetAllAsync_SearchByMatricule_FiltersResults()
    {
        var svc = CreateService(out _);
        var f = MakeFournisseur("FO00001", "Alpha");
        f.MatriculeFiscale = "MAT123456";
        await svc.AddAsync(f);
        await svc.AddAsync(MakeFournisseur("FO00002", "Beta"));

        var results = await svc.GetAllAsync("MAT123456");

        Assert.Single(results);
        Assert.Equal("FO00001", results[0].CodeFournisseur);
    }

    [Fact]
    public async Task GetAllAsync_EmptySearch_ReturnsAll()
    {
        var svc = CreateService(out _);
        await svc.AddAsync(MakeFournisseur("FO00001", "Alpha"));
        await svc.AddAsync(MakeFournisseur("FO00002", "Beta"));

        var results = await svc.GetAllAsync("  ");

        Assert.Equal(2, results.Count);
    }

    // ── GetByCode ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByCodeAsync_ExistingCode_ReturnsFournisseur()
    {
        var svc = CreateService(out _);
        await svc.AddAsync(MakeFournisseur("FO00001", "Test Ltd"));

        var result = await svc.GetByCodeAsync("FO00001");

        Assert.NotNull(result);
        Assert.Equal("Test Ltd", result.NomFournisseur);
    }

    [Fact]
    public async Task GetByCodeAsync_UnknownCode_ReturnsNull()
    {
        var svc = CreateService(out _);
        Assert.Null(await svc.GetByCodeAsync("UNKNOWN"));
    }

    // ── Update ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ChangesNomFournisseur()
    {
        var svc = CreateService(out var db);
        await svc.AddAsync(MakeFournisseur("FO00001", "Old Name"));

        var f = await db.Fournisseurs.FindAsync("FO00001");
        f!.NomFournisseur = "New Name";
        await svc.UpdateAsync(f);

        Assert.Equal("New Name", (await db.Fournisseurs.FindAsync("FO00001"))!.NomFournisseur);
    }

    [Fact]
    public async Task UpdateAsync_AfterFormLoadsReferenceListsAndGetByCode_DoesNotThrowIdentityConflict()
    {
        // Reproduces FournisseurForm.razor's OnInitializedAsync: it loads the Devise dropdown
        // (tracked, no AsNoTracking) and THEN GetByCodeAsync (AsNoTracking + Include(Devise)) in
        // the same DbContext scope — Update() previously tried to re-attach the AsNoTracking
        // graph's Devise instance, colliding with the already-tracked dropdown-list instance.
        var svc = CreateService(out var db);
        await svc.AddAsync(MakeFournisseur("FO00001", "Old Name"));
        db.ChangeTracker.Clear(); // simulates a fresh circuit that never touched this row before

        await db.Devises.ToListAsync(); // simulates the form's currency dropdown load (tracked)
        var fournisseur = await svc.GetByCodeAsync("FO00001"); // AsNoTracking + Include(Devise)
        fournisseur!.NomFournisseur = "New Name";

        var ex = await Record.ExceptionAsync(() => svc.UpdateAsync(fournisseur));

        Assert.Null(ex);
        Assert.Equal("New Name", (await db.Fournisseurs.FindAsync("FO00001"))!.NomFournisseur);
    }

    [Fact]
    public async Task UpdateAsync_ImmediatelyAfterAddAsync_InSameCircuit_DoesNotThrowIdentityConflict()
    {
        // Real Blazor Server bug: a scoped DbContext lives for the whole circuit — it is NEVER
        // cleared between actions (unlike the test above, which calls ChangeTracker.Clear() to
        // simulate a fresh circuit and therefore never exercised this path). Adding a fournisseur
        // then immediately editing it in the same browser session left the Added entity tracked;
        // GetByCodeAsync's AsNoTracking() instance then collided with it on Update().
        var svc = CreateService(out var db);
        await svc.AddAsync(MakeFournisseur("FO00001", "Old Name"));

        var fournisseur = await svc.GetByCodeAsync("FO00001");
        fournisseur!.NomFournisseur = "New Name";

        var ex2 = await Record.ExceptionAsync(() => svc.UpdateAsync(fournisseur));

        Assert.Null(ex2);
        Assert.Equal("New Name", (await db.Fournisseurs.FindAsync("FO00001"))!.NomFournisseur);
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingCode_RemovesFournisseur()
    {
        var svc = CreateService(out var db);
        await svc.AddAsync(MakeFournisseur("FO00001", "To Delete"));

        await svc.DeleteAsync("FO00001");

        Assert.Equal(0, await db.Fournisseurs.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_UnknownCode_DoesNotThrow()
    {
        var svc = CreateService(out _);

        var ex = await Record.ExceptionAsync(() => svc.DeleteAsync("UNKNOWN"));

        Assert.Null(ex);
    }
}
