using Microsoft.EntityFrameworkCore;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;
using Web_GestCom.Tests.Helpers;
using Xunit;

namespace Web_GestCom.Tests.Services;

public class ProduitServiceTests
{
    private static ProduitService CreateService(out AppDbContext db)
    {
        db = DbContextFactory.Create();
        return new ProduitService(db, new NoOpJournalActiviteService());
    }

    private static Produit MakeProduit(string code, string designation, double quantite = 100, double stockMinimal = 10)
        => new()
        {
            CodeProduit = code,
            DesignationProduit = designation,
            CodeDevise = 1,
            CodeUniteProduit = 1,
            CodeTvaProduit = 1,
            CodeCategorieProduit = 1,
            CodeFabriquantProduit = 1,
            Quantite = quantite,
            StockMinimal = stockMinimal
        };

    // ── Update ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_AfterFormLoadsReferenceListsAndGetByCode_DoesNotThrowIdentityConflict()
    {
        // Reproduces ProduitForm.razor's OnInitializedAsync: it loads the dropdown reference lists
        // (tracked, no AsNoTracking) and THEN GetByCodeAsync (AsNoTracking + Include navigations)
        // in the same DbContext scope — Update() previously tried to re-attach the AsNoTracking
        // graph's navigation instances, colliding with the already-tracked dropdown-list instances.
        var svc = CreateService(out var db);
        await svc.AddAsync(MakeProduit("PR00001", "Clavier USB"));
        db.ChangeTracker.Clear(); // simulates a fresh circuit that never touched this row before

        await db.CategoriesProduit.ToListAsync(); // simulates the form's dropdown load (tracked)
        var produit = await svc.GetByCodeAsync("PR00001"); // AsNoTracking + Include(CategorieProduit)
        produit!.DesignationProduit = "Clavier USB Sans Fil";

        var ex = await Record.ExceptionAsync(() => svc.UpdateAsync(produit));

        Assert.Null(ex);
        Assert.Equal("Clavier USB Sans Fil", (await db.Produits.FindAsync("PR00001"))!.DesignationProduit);
    }

    [Fact]
    public async Task UpdateAsync_ImmediatelyAfterAddAsync_InSameCircuit_DoesNotThrowIdentityConflict()
    {
        // Real Blazor Server bug: a scoped DbContext lives for the whole circuit — it is NEVER
        // cleared between actions (unlike the test above, which calls ChangeTracker.Clear() to
        // simulate a fresh circuit and therefore never exercised this path). Adding a product then
        // immediately editing it in the same browser session — a completely normal user flow —
        // left the Added entity tracked; GetByCodeAsync's AsNoTracking() instance then collided
        // with it on Update(), throwing "The instance of entity type 'Produit' cannot be tracked
        // because another instance with the same key value ... is already being tracked."
        var svc = CreateService(out var db);
        await svc.AddAsync(MakeProduit("PR00001", "Clavier USB"));

        var produit = await svc.GetByCodeAsync("PR00001");
        produit!.DesignationProduit = "Clavier USB Sans Fil";

        var ex = await Record.ExceptionAsync(() => svc.UpdateAsync(produit));

        Assert.Null(ex);
        Assert.Equal("Clavier USB Sans Fil", (await db.Produits.FindAsync("PR00001"))!.DesignationProduit);
    }

    // ── Add ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_WithExplicitCode_PersistsProduit()
    {
        var svc = CreateService(out var db);

        var code = await svc.AddAsync(MakeProduit("PR00001", "Clavier USB"));

        Assert.Equal("PR00001", code);
        Assert.Equal(1, await db.Produits.CountAsync());
    }

    [Fact]
    public async Task AddAsync_WithoutCode_AutoGeneratesFirstCode()
    {
        var svc = CreateService(out _);
        var p = MakeProduit("", "Auto Produit");

        var code = await svc.AddAsync(p);

        Assert.Equal("PR00001", code);
    }

    [Fact]
    public async Task AddAsync_SecondProduitWithoutCode_IncrementsNumber()
    {
        var svc = CreateService(out _);
        await svc.AddAsync(MakeProduit("PR00001", "First"));

        var code = await svc.AddAsync(MakeProduit("", "Second"));

        Assert.Equal("PR00002", code);
    }

    [Fact]
    public async Task AddAsync_AfterDeletingAMiddleProduit_DoesNotCollideWithTheSurvivingHighestCode()
    {
        // Real production bug: code generation used COUNT(*) + 1. With PR00001/02/03 present
        // (count=3) and PR00002 deleted (count=2), the next auto-generated code recomputed
        // count+1 = "PR00003" — which still existed (PR00001 and PR00003 survived the delete) —
        // and SaveChangesAsync threw "Violation of PRIMARY KEY constraint 'PK_produit'. Cannot
        // insert duplicate key ... (PR00003)." (the reported production error used PR00001, but
        // the mechanism is identical). Must be based on the MAX existing number, not the row
        // count, so a gap left by a deleted row never causes the next code to collide with a
        // still-surviving higher-numbered row.
        var svc = CreateService(out _);
        await svc.AddAsync(MakeProduit("PR00001", "First"));
        await svc.AddAsync(MakeProduit("PR00002", "Second"));
        await svc.AddAsync(MakeProduit("PR00003", "Third"));
        await svc.DeleteAsync("PR00002");

        var code = await svc.AddAsync(MakeProduit("", "Fourth"));

        Assert.Equal("PR00004", code);
    }

    // ── GetAll ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_NoFilter_ReturnsAllOrderedByDesignation()
    {
        var svc = CreateService(out _);
        await svc.AddAsync(MakeProduit("PR00002", "Zoulou"));
        await svc.AddAsync(MakeProduit("PR00001", "Alpha"));

        var results = await svc.GetAllAsync();

        Assert.Equal(2, results.Count);
        Assert.Equal("Alpha", results[0].DesignationProduit);
        Assert.Equal("Zoulou", results[1].DesignationProduit);
    }

    [Fact]
    public async Task GetAllAsync_SearchByDesignation_FiltersResults()
    {
        var svc = CreateService(out _);
        await svc.AddAsync(MakeProduit("PR00001", "Clavier USB"));
        await svc.AddAsync(MakeProduit("PR00002", "Souris optique"));

        var results = await svc.GetAllAsync("Clavier");

        Assert.Single(results);
        Assert.Equal("Clavier USB", results[0].DesignationProduit);
    }

    [Fact]
    public async Task GetAllAsync_SearchByCode_FiltersResults()
    {
        var svc = CreateService(out _);
        await svc.AddAsync(MakeProduit("PR00001", "Clavier USB"));
        await svc.AddAsync(MakeProduit("PR00002", "Souris optique"));

        var results = await svc.GetAllAsync("PR00001");

        Assert.Single(results);
        Assert.Equal("PR00001", results[0].CodeProduit);
    }

    [Fact]
    public async Task GetAllAsync_WithCategorieFilter_FiltersResults()
    {
        var svc = CreateService(out var db);
        db.CategoriesProduit.Add(new CategorieProduit { CodeCategorieProduit = 2, NomCategorieProduit = "Informatique" });
        await db.SaveChangesAsync();

        var p1 = MakeProduit("PR00001", "Général A"); p1.CodeCategorieProduit = 1;
        var p2 = MakeProduit("PR00002", "Info B");    p2.CodeCategorieProduit = 2;
        await svc.AddAsync(p1);
        await svc.AddAsync(p2);

        var results = await svc.GetAllAsync(categorieCode: 2);

        Assert.Single(results);
        Assert.Equal("PR00002", results[0].CodeProduit);
    }

    // ── GetByCode ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByCodeAsync_ExistingCode_ReturnsProduit()
    {
        var svc = CreateService(out _);
        await svc.AddAsync(MakeProduit("PR00001", "Test Produit"));

        var result = await svc.GetByCodeAsync("PR00001");

        Assert.NotNull(result);
        Assert.Equal("Test Produit", result.DesignationProduit);
    }

    [Fact]
    public async Task GetByCodeAsync_UnknownCode_ReturnsNull()
    {
        var svc = CreateService(out _);
        Assert.Null(await svc.GetByCodeAsync("UNKNOWN"));
    }

    // ── Update ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ChangesDesignation()
    {
        var svc = CreateService(out var db);
        await svc.AddAsync(MakeProduit("PR00001", "Old Name"));

        var p = await db.Produits.FindAsync("PR00001");
        p!.DesignationProduit = "New Name";
        await svc.UpdateAsync(p);

        Assert.Equal("New Name", (await db.Produits.FindAsync("PR00001"))!.DesignationProduit);
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingCode_RemovesProduit()
    {
        var svc = CreateService(out var db);
        await svc.AddAsync(MakeProduit("PR00001", "To Delete"));

        await svc.DeleteAsync("PR00001");

        Assert.Equal(0, await db.Produits.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_UnknownCode_DoesNotThrow()
    {
        var svc = CreateService(out _);

        var ex = await Record.ExceptionAsync(() => svc.DeleteAsync("UNKNOWN"));

        Assert.Null(ex);
    }

    // ── UpdateStock ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStockAsync_PositiveDelta_IncreasesQuantite()
    {
        var svc = CreateService(out var db);
        await svc.AddAsync(MakeProduit("PR00001", "Test", quantite: 50));

        await svc.UpdateStockAsync("PR00001", 20);

        Assert.Equal(70, (await db.Produits.FindAsync("PR00001"))!.Quantite);
    }

    [Fact]
    public async Task UpdateStockAsync_NegativeDelta_DecreasesQuantite()
    {
        var svc = CreateService(out var db);
        await svc.AddAsync(MakeProduit("PR00001", "Test", quantite: 50));

        await svc.UpdateStockAsync("PR00001", -15);

        Assert.Equal(35, (await db.Produits.FindAsync("PR00001"))!.Quantite);
    }

    [Fact]
    public async Task UpdateStockAsync_UnknownCode_DoesNotThrow()
    {
        var svc = CreateService(out _);

        var ex = await Record.ExceptionAsync(() => svc.UpdateStockAsync("UNKNOWN", 10));

        Assert.Null(ex);
    }

    // ── Stock Alerte ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStockAlerteAsync_ReturnsBelowOrAtMinimum()
    {
        var svc = CreateService(out _);
        await svc.AddAsync(MakeProduit("PR00001", "Normal",      quantite: 100, stockMinimal: 10));
        await svc.AddAsync(MakeProduit("PR00002", "En Alerte",   quantite: 5,   stockMinimal: 10));
        await svc.AddAsync(MakeProduit("PR00003", "Au Minimum",  quantite: 10,  stockMinimal: 10));

        var alerts = await svc.GetStockAlerteAsync();

        // PR00002 (5 <= 10) and PR00003 (10 <= 10) both included; PR00001 excluded
        Assert.Equal(2, alerts.Count);
        Assert.DoesNotContain(alerts, p => p.CodeProduit == "PR00001");
    }

    [Fact]
    public async Task GetStockAlerteAsync_EmptyTable_ReturnsEmptyList()
    {
        var svc = CreateService(out _);
        Assert.Empty(await svc.GetStockAlerteAsync());
    }

    [Fact]
    public async Task GetStockAlerteAsync_ReturnsResultsOrderedByDesignation()
    {
        var svc = CreateService(out _);
        await svc.AddAsync(MakeProduit("PR00002", "Zoulou Alerte", quantite: 1, stockMinimal: 10));
        await svc.AddAsync(MakeProduit("PR00001", "Alpha Alerte",  quantite: 1, stockMinimal: 10));

        var alerts = await svc.GetStockAlerteAsync();

        Assert.Equal("Alpha Alerte", alerts[0].DesignationProduit);
    }
}
