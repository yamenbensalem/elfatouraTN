using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;
using Web_T4C_GestCom.Tests.Helpers;
using Xunit;

namespace Web_T4C_GestCom.Tests.Services;

public class BonLivraisonServiceTests
{
    private static (BonLivraisonService svc, AppDbContext db) CreateService()
    {
        var db = DbContextFactory.Create();
        var journal = new NoOpJournalActiviteService();
        var produitService = new ProduitService(db, journal);
        var numService = new DocumentNumberService(db);
        return (new BonLivraisonService(db, numService, produitService, journal), db);
    }

    private static async Task<Produit> SeedBasicData(AppDbContext db)
    {
        db.Clients.Add(new Client { CodeClient = "CL00001", NomClient = "Test Client", CodeDevise = 1 });
        var produit = new Produit
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
        };
        db.Produits.Add(produit);
        await db.SaveChangesAsync();
        return produit;
    }

    private static BonLivraison MakeBon(string codeClient = "CL00001")
        => new() { CodeClient = codeClient, DateBonLivraison = DateTime.Today };

    private static LigneBonLivraison MakeLigne(string codeProduit, double qte, double pu, double tva = 19)
        => new() { CodeProduit = codeProduit, Quantite = qte, PrixUnitaire = pu, MontantHT = qte * pu, Tva = tva };

    [Fact]
    public async Task CreateAsync_AssignsAutoNumeroAndPersists()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);

        var result = await svc.CreateAsync(MakeBon(), [MakeLigne("PR00001", 2, 100)]);

        Assert.StartsWith("BL", result.NumeroBonLivraison);
        Assert.Equal(1, await db.BonsLivraison.CountAsync());
        Assert.Equal(1, await db.LignesBonLivraison.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_DecreasesProductStock()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);

        await svc.CreateAsync(MakeBon(), [MakeLigne("PR00001", 10, 50)]);

        Assert.Equal(90, (await db.Produits.FindAsync("PR00001"))!.Quantite);
    }

    [Fact]
    public async Task DeleteAsync_RestoresStockAndRemovesBon()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);
        var created = await svc.CreateAsync(MakeBon(), [MakeLigne("PR00001", 10, 50)]);
        Assert.Equal(90, (await db.Produits.FindAsync("PR00001"))!.Quantite);

        await svc.DeleteAsync(created.NumeroBonLivraison);

        Assert.Equal(100, (await db.Produits.FindAsync("PR00001"))!.Quantite);
        Assert.Equal(0, await db.BonsLivraison.CountAsync());
        Assert.Equal(0, await db.LignesBonLivraison.CountAsync());
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

        var created = await svc.CreateAsync(MakeBon(), [MakeLigne("PR00001", 5, 100)]);
        Assert.Equal(95, (await db.Produits.FindAsync("PR00001"))!.Quantite);

        await svc.UpdateAsync(created, [MakeLigne("PR00002", 3, 80)]);

        Assert.Equal(100, (await db.Produits.FindAsync("PR00001"))!.Quantite);
        Assert.Equal(47, (await db.Produits.FindAsync("PR00002"))!.Quantite);
        Assert.Equal(1, await db.LignesBonLivraison.CountAsync());
        Assert.Equal("PR00002", (await db.LignesBonLivraison.FirstAsync()).CodeProduit);
    }

    [Fact]
    public async Task CloneAsync_CopiesLinesAndDecreasesStockAgain()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);
        var created = await svc.CreateAsync(MakeBon(), [MakeLigne("PR00001", 5, 100)]);
        Assert.Equal(95, (await db.Produits.FindAsync("PR00001"))!.Quantite);

        var clone = await svc.CloneAsync(created.NumeroBonLivraison);

        Assert.NotEqual(created.NumeroBonLivraison, clone.NumeroBonLivraison);
        Assert.Equal(90, (await db.Produits.FindAsync("PR00001"))!.Quantite);
        var cloneLines = await db.LignesBonLivraison.Where(l => l.NumeroBonLivraison == clone.NumeroBonLivraison).ToListAsync();
        Assert.Single(cloneLines);
    }

    [Fact]
    public async Task GetAllAsync_FilterByClientCode_ReturnsOnlyThatClient()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);
        db.Clients.Add(new Client { CodeClient = "CL00002", NomClient = "Other", CodeDevise = 1 });
        await db.SaveChangesAsync();

        await svc.CreateAsync(MakeBon("CL00001"), []);
        await svc.CreateAsync(MakeBon("CL00002"), []);

        var results = await svc.GetAllAsync(clientCode: "CL00001");

        Assert.Single(results);
        Assert.Equal("CL00001", results[0].CodeClient);
    }

    [Fact]
    public async Task GetByNumeroAsync_UnknownNumero_ReturnsNull()
    {
        var (svc, _) = CreateService();
        Assert.Null(await svc.GetByNumeroAsync("UNKNOWN"));
    }
}
