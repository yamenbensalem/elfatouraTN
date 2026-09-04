using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;
using Web_T4C_GestCom.Tests.Helpers;
using Xunit;

namespace Web_T4C_GestCom.Tests.Services;

public class BonReceptionServiceTests
{
    private static (BonReceptionService svc, AppDbContext db) CreateService()
    {
        var db = DbContextFactory.Create();
        var numService = new DocumentNumberService(db);
        return (new BonReceptionService(db, numService, new NoOpJournalActiviteService()), db);
    }

    private static async Task SeedBasicData(AppDbContext db)
    {
        db.Fournisseurs.Add(new Fournisseur { CodeFournisseur = "FR00001", NomFournisseur = "Test Fournisseur", CodeDevise = 1 });
        db.Produits.Add(new Produit
        {
            CodeProduit = "PR00001",
            DesignationProduit = "Test Produit",
            CodeDevise = 1,
            CodeUniteProduit = 1,
            CodeTvaProduit = 1,
            CodeCategorieProduit = 1,
            CodeFabriquantProduit = 1,
            Quantite = 100,
            StockMinimal = 5
        });
        await db.SaveChangesAsync();
    }

    private static BonReception MakeBon(string codeFournisseur = "FR00001")
        => new() { CodeFournisseur = codeFournisseur, DateBonReception = DateTime.Today };

    private static LigneBonReception MakeLigne(string codeProduit, double qte, double pu, double tva = 19)
        => new() { CodeProduit = codeProduit, Quantite = qte, PrixUnitaire = pu, MontantHT = qte * pu, Tva = tva };

    [Fact]
    public async Task CreateAsync_AssignsAutoNumeroAndIncreasesStock()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);

        var result = await svc.CreateAsync(MakeBon(), [MakeLigne("PR00001", 10, 50)]);

        Assert.StartsWith("BR", result.NumeroBonReception);
        Assert.Equal(110, (await db.Produits.FindAsync("PR00001"))!.Quantite);
    }

    [Fact]
    public async Task DeleteAsync_RestoresStockDownAndRemovesBon()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);
        var created = await svc.CreateAsync(MakeBon(), [MakeLigne("PR00001", 10, 50)]);
        Assert.Equal(110, (await db.Produits.FindAsync("PR00001"))!.Quantite);

        await svc.DeleteAsync(created.NumeroBonReception);

        Assert.Equal(100, (await db.Produits.FindAsync("PR00001"))!.Quantite);
        Assert.Equal(0, await db.BonsReception.CountAsync());
    }

    [Fact]
    public async Task UpdateAsync_ReplacesLignesAndAdjustsStock()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);
        db.Produits.Add(new Produit
        {
            CodeProduit = "PR00002", DesignationProduit = "Second",
            CodeDevise = 1, CodeUniteProduit = 1, CodeTvaProduit = 1,
            CodeCategorieProduit = 1, CodeFabriquantProduit = 1,
            Quantite = 50, StockMinimal = 5
        });
        await db.SaveChangesAsync();

        var created = await svc.CreateAsync(MakeBon(), [MakeLigne("PR00001", 10, 50)]);
        Assert.Equal(110, (await db.Produits.FindAsync("PR00001"))!.Quantite);

        await svc.UpdateAsync(created, [MakeLigne("PR00002", 5, 80)]);

        Assert.Equal(100, (await db.Produits.FindAsync("PR00001"))!.Quantite);
        Assert.Equal(55, (await db.Produits.FindAsync("PR00002"))!.Quantite);
        var lignes = await db.LignesBonReception.Where(l => l.NumeroBonReception == created.NumeroBonReception).ToListAsync();
        Assert.Single(lignes);
        Assert.Equal("PR00002", lignes[0].CodeProduit);
    }

    [Fact]
    public async Task CloneAsync_CopiesLinesAndIncreasesStockAgain()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);
        var created = await svc.CreateAsync(MakeBon(), [MakeLigne("PR00001", 10, 50)]);
        Assert.Equal(110, (await db.Produits.FindAsync("PR00001"))!.Quantite);

        var clone = await svc.CloneAsync(created.NumeroBonReception);

        Assert.NotEqual(created.NumeroBonReception, clone.NumeroBonReception);
        Assert.Equal(120, (await db.Produits.FindAsync("PR00001"))!.Quantite);
    }

    [Fact]
    public async Task GetByNumeroAsync_UnknownNumero_ReturnsNull()
    {
        var (svc, _) = CreateService();
        Assert.Null(await svc.GetByNumeroAsync("UNKNOWN"));
    }

    [Fact]
    public async Task DeleteAsync_UnknownNumero_Throws()
    {
        var (svc, _) = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.DeleteAsync("UNKNOWN"));
    }
}
