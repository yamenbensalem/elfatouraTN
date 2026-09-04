using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;
using Web_T4C_GestCom.Tests.Helpers;
using Xunit;

namespace Web_T4C_GestCom.Tests.Services;

public class FactureFournisseurServiceTests
{
    private static (FactureFournisseurService svc, AppDbContext db) CreateService()
    {
        var db = DbContextFactory.Create();
        return (new FactureFournisseurService(db, new DocumentNumberService(db), new NoOpJournalActiviteService()), db);
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

    private static FactureFournisseur MakeFacture(string codeFournisseur = "FR00001")
        => new() { CodeFournisseur = codeFournisseur, DateFactureFournisseur = DateTime.Today };

    private static LigneFactureFournisseur MakeLigne(string codeProduit, double qte, double pu, double tva = 19)
        => new() { CodeProduit = codeProduit, Quantite = qte, PrixUnitaire = pu, MontantHT = qte * pu, Tva = tva };

    // ── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_AssignsAutoNumeroAndIncreasesStock()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);

        var result = await svc.CreateAsync(MakeFacture(), [MakeLigne("PR00001", 10, 50)]);

        Assert.StartsWith("FF", result.NumeroFactureFournisseur);
        Assert.Equal(110, (await db.Produits.FindAsync("PR00001"))!.Quantite);
    }

    [Fact]
    public async Task CreateAsync_RecalculatesTotalsCorrectly()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);

        var result = await svc.CreateAsync(MakeFacture(), [MakeLigne("PR00001", 2, 100, tva: 19)]);

        Assert.Equal(200, result.MontantHT);
        Assert.Equal(38, result.MontantTVA);
        Assert.Equal(238, result.MontantTTC);
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_RestoresStockDownAndRemovesFacture()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);
        var created = await svc.CreateAsync(MakeFacture(), [MakeLigne("PR00001", 10, 50)]);
        Assert.Equal(110, (await db.Produits.FindAsync("PR00001"))!.Quantite);

        await svc.DeleteAsync(created.NumeroFactureFournisseur);

        Assert.Equal(100, (await db.Produits.FindAsync("PR00001"))!.Quantite);
        Assert.Equal(0, await db.FacturesFournisseur.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_AlsoRemovesReglements()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);
        var created = await svc.CreateAsync(MakeFacture(), []);
        await svc.AddReglementAsync(new ReglementFactureFournisseur
        {
            NumeroFactureFournisseur = created.NumeroFactureFournisseur,
            Montant = 50,
            DateReglement = DateTime.Today,
            CodeModePayement = 1
        });

        await svc.DeleteAsync(created.NumeroFactureFournisseur);

        Assert.Equal(0, await db.ReglementsFactureFournisseur.CountAsync());
    }

    // ── Update ───────────────────────────────────────────────────────────────

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

        var created = await svc.CreateAsync(MakeFacture(), [MakeLigne("PR00001", 10, 50)]);
        Assert.Equal(110, (await db.Produits.FindAsync("PR00001"))!.Quantite);

        await svc.UpdateAsync(created, [MakeLigne("PR00002", 5, 80)]);

        Assert.Equal(100, (await db.Produits.FindAsync("PR00001"))!.Quantite);
        Assert.Equal(55, (await db.Produits.FindAsync("PR00002"))!.Quantite);
    }

    // ── Reglement / Solde ────────────────────────────────────────────────────

    [Fact]
    public async Task AddReglementAsync_PartialPayment_SetsPartiellementRegle()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);
        var created = await svc.CreateAsync(MakeFacture(), [MakeLigne("PR00001", 1, 500, tva: 0)]);

        await svc.AddReglementAsync(new ReglementFactureFournisseur
        {
            NumeroFactureFournisseur = created.NumeroFactureFournisseur,
            Montant = 100,
            DateReglement = DateTime.Today,
            CodeModePayement = 1
        });

        Assert.Equal("Partiellement Réglé", (await db.FacturesFournisseur.FindAsync(created.NumeroFactureFournisseur))!.EtatReglement);
    }

    [Fact]
    public async Task AddReglementAsync_FullPayment_SetsRegle()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);
        var created = await svc.CreateAsync(MakeFacture(), [MakeLigne("PR00001", 1, 100, tva: 0)]);

        await svc.AddReglementAsync(new ReglementFactureFournisseur
        {
            NumeroFactureFournisseur = created.NumeroFactureFournisseur,
            Montant = 100,
            DateReglement = DateTime.Today,
            CodeModePayement = 1
        });

        Assert.Equal("Réglé", (await db.FacturesFournisseur.FindAsync(created.NumeroFactureFournisseur))!.EtatReglement);
    }

    [Fact]
    public async Task GetSoldeAsync_AfterPartialPayment_ReturnsRemainder()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);
        var created = await svc.CreateAsync(MakeFacture(), [MakeLigne("PR00001", 1, 200, tva: 0)]);
        await svc.AddReglementAsync(new ReglementFactureFournisseur
        {
            NumeroFactureFournisseur = created.NumeroFactureFournisseur,
            Montant = 50,
            DateReglement = DateTime.Today,
            CodeModePayement = 1
        });

        var solde = await svc.GetSoldeAsync(created.NumeroFactureFournisseur);

        Assert.Equal(150, solde);
    }

    [Fact]
    public async Task GetSoldeAsync_UnknownNumero_ReturnsZero()
    {
        var (svc, _) = CreateService();
        Assert.Equal(0, await svc.GetSoldeAsync("UNKNOWN"));
    }

    // ── Clone ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CloneAsync_CopiesLinesAndIncreasesStockAgain()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);
        var created = await svc.CreateAsync(MakeFacture(), [MakeLigne("PR00001", 10, 50)]);
        Assert.Equal(110, (await db.Produits.FindAsync("PR00001"))!.Quantite);

        var clone = await svc.CloneAsync(created.NumeroFactureFournisseur);

        Assert.NotEqual(created.NumeroFactureFournisseur, clone.NumeroFactureFournisseur);
        Assert.Equal(120, (await db.Produits.FindAsync("PR00001"))!.Quantite);
        Assert.Equal("Non Réglé", clone.EtatReglement);
    }
}
