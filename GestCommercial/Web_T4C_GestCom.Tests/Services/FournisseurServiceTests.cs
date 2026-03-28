using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;
using Web_T4C_GestCom.Tests.Helpers;
using Xunit;

namespace Web_T4C_GestCom.Tests.Services;

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
