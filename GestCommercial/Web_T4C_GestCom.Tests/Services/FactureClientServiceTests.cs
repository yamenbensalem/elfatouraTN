using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;
using Web_T4C_GestCom.Tests.Helpers;
using Xunit;

namespace Web_T4C_GestCom.Tests.Services;

public class FactureClientServiceTests
{
    private static string YM => DateTime.Today.ToString("yyyyMM");

    /// <summary>
    /// Builds the full service graph sharing a single in-memory DbContext.
    /// </summary>
    private static (FactureClientService svc, AppDbContext db, AppConfigService config) CreateService()
    {
        var db = DbContextFactory.Create();
        var journal = new NoOpJournalActiviteService();
        var produitService = new ProduitService(db, journal);
        var numService = new DocumentNumberService(db);
        var config = new AppConfigService(
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AppConfig:TimbreFiscal"] = "0.6",
                    ["AppConfig:TauxRetenue"] = "1.5"
                })
                .Build());
        return (new FactureClientService(db, numService, produitService, journal), db, config);
    }

    private static async Task<(Client client, Produit produit)> SeedBasicData(AppDbContext db)
    {
        var client = new Client { CodeClient = "CL00001", NomClient = "Test Client", CodeDevise = 1 };
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
        db.Clients.Add(client);
        db.Produits.Add(produit);
        await db.SaveChangesAsync();
        return (client, produit);
    }

    private static FactureClient MakeFacture(string codeClient = "CL00001")
        => new() { CodeClient = codeClient, DateFactureClient = DateTime.Today };

    private static LigneFactureClient MakeLigne(string codeProduit, double qte, double pu, double tva = 19, double fodec = 0)
        => new() { CodeProduit = codeProduit, Quantite = qte, PrixUnitaire = pu, MontantHT = qte * pu, Tva = tva, Fodec = fodec };

    // ── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_AssignsAutoNumeroAndPersists()
    {
        var (svc, db, config) = CreateService();
        await SeedBasicData(db);

        var result = await svc.CreateAsync(MakeFacture(), [MakeLigne("PR00001", 2, 100)], config);

        Assert.Equal($"FC{YM}001", result.NumeroFactureClient);
        Assert.Equal(1, await db.FacturesClient.CountAsync());
        Assert.Equal(1, await db.LignesFactureClient.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_LineWithNegativePrixUnitaire_Throws()
    {
        var (svc, db, config) = CreateService();
        await SeedBasicData(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.CreateAsync(MakeFacture(), [MakeLigne("PR00001", 2, -100)], config));
    }

    [Fact]
    public async Task CreateAsync_DecreasesProductStock()
    {
        var (svc, db, config) = CreateService();
        await SeedBasicData(db);

        await svc.CreateAsync(MakeFacture(), [MakeLigne("PR00001", 10, 50)], config);

        Assert.Equal(90, (await db.Produits.FindAsync("PR00001"))!.Quantite); // 100 - 10
    }

    [Fact]
    public async Task CreateAsync_SetsTimbreFromConfig()
    {
        var (svc, db, config) = CreateService();
        await SeedBasicData(db);

        var result = await svc.CreateAsync(MakeFacture(), [], config);

        Assert.Equal(0.6, result.Timbre);
    }

    [Fact]
    public async Task CreateAsync_RecalculatesTotalsCorrectly()
    {
        // ligne: qte=2, pu=100 → MontantHT=200, Tva=19%, Fodec=1%, Remise facture=10%
        // Fodec     = 1%  * 200 = 2
        // TVA       = 19% * 200 = 38
        // Remise    = 10% * 200 = 20
        // TTC       = 200 - 20 + 2 + 38 = 220
        var (svc, db, config) = CreateService();
        await SeedBasicData(db);
        var facture = MakeFacture(); facture.Remise = 10;
        var ligne = MakeLigne("PR00001", 2, 100, tva: 19, fodec: 1);
        ligne.MontantHT = 200;

        var result = await svc.CreateAsync(facture, [ligne], config);

        Assert.Equal(200, result.MontantHT);
        Assert.Equal(2,   result.Fodec);
        Assert.Equal(38,  result.MontantTVA);
        Assert.Equal(220, result.MontantTTC);
    }

    [Fact]
    public async Task CreateAsync_SecondFacture_GetsNextSequentialNumber()
    {
        var (svc, db, config) = CreateService();
        await SeedBasicData(db);

        await svc.CreateAsync(MakeFacture(), [], config);
        var second = await svc.CreateAsync(MakeFacture(), [], config);

        Assert.Equal($"FC{YM}002", second.NumeroFactureClient);
    }

    // ── GetAll ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyNonAvoirs()
    {
        var (svc, db, config) = CreateService();
        await SeedBasicData(db);
        var avoir = MakeFacture(); avoir.IsAvoir = true;
        await svc.CreateAsync(MakeFacture(), [], config);
        await svc.CreateAsync(avoir, [], config);

        var factures = await svc.GetAllAsync(avoirsOnly: false);

        Assert.Single(factures);
        Assert.False(factures[0].IsAvoir);
    }

    [Fact]
    public async Task GetAllAsync_AvoirsOnly_ReturnsOnlyAvoirs()
    {
        var (svc, db, config) = CreateService();
        await SeedBasicData(db);
        var avoir = MakeFacture(); avoir.IsAvoir = true;
        await svc.CreateAsync(MakeFacture(), [], config);
        await svc.CreateAsync(avoir, [], config);

        var avoirs = await svc.GetAllAsync(avoirsOnly: true);

        Assert.Single(avoirs);
        Assert.True(avoirs[0].IsAvoir);
    }

    [Fact]
    public async Task GetAllAsync_FilterByClientCode_ReturnsOnlyThatClient()
    {
        var (svc, db, config) = CreateService();
        await SeedBasicData(db);
        db.Clients.Add(new Client { CodeClient = "CL00002", NomClient = "Other", CodeDevise = 1 });
        await db.SaveChangesAsync();

        await svc.CreateAsync(MakeFacture("CL00001"), [], config);
        await svc.CreateAsync(MakeFacture("CL00002"), [], config);

        var results = await svc.GetAllAsync(clientCode: "CL00001");

        Assert.Single(results);
        Assert.Equal("CL00001", results[0].CodeClient);
    }

    // ── GetByNumero ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByNumeroAsync_ReturnsFactureWithLignesAndReglements()
    {
        var (svc, db, config) = CreateService();
        await SeedBasicData(db);
        var created = await svc.CreateAsync(MakeFacture(), [MakeLigne("PR00001", 3, 50)], config);

        var result = await svc.GetByNumeroAsync(created.NumeroFactureClient);

        Assert.NotNull(result);
        Assert.Single(result.Lignes);
        Assert.Equal("PR00001", result.Lignes.First().CodeProduit);
    }

    [Fact]
    public async Task GetByNumeroAsync_UnknownNumero_ReturnsNull()
    {
        var (svc, _, _) = CreateService();
        Assert.Null(await svc.GetByNumeroAsync("UNKNOWN"));
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_RemovesFactureAndLines()
    {
        var (svc, db, config) = CreateService();
        await SeedBasicData(db);
        var created = await svc.CreateAsync(MakeFacture(), [MakeLigne("PR00001", 5, 100)], config);

        await svc.DeleteAsync(created.NumeroFactureClient);

        Assert.Equal(0, await db.FacturesClient.CountAsync());
        Assert.Equal(0, await db.LignesFactureClient.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_RestoresStockFromLines()
    {
        var (svc, db, config) = CreateService();
        await SeedBasicData(db);
        var created = await svc.CreateAsync(MakeFacture(), [MakeLigne("PR00001", 5, 100)], config);
        Assert.Equal(95, (await db.Produits.FindAsync("PR00001"))!.Quantite);

        await svc.DeleteAsync(created.NumeroFactureClient);

        Assert.Equal(100, (await db.Produits.FindAsync("PR00001"))!.Quantite);
    }

    [Fact]
    public async Task DeleteAsync_AlsoRemovesReglements()
    {
        var (svc, db, config) = CreateService();
        await SeedBasicData(db);
        var created = await svc.CreateAsync(MakeFacture(), [], config);
        await svc.AddReglementAsync(new ReglementFactureClient
        {
            NumeroFactureClient = created.NumeroFactureClient,
            Montant = 50,
            DateReglement = DateTime.Today,
            CodeModePayement = 1
        });

        await svc.DeleteAsync(created.NumeroFactureClient);

        Assert.Equal(0, await db.ReglementsFactureClient.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_UnknownNumero_DoesNotThrow()
    {
        var (svc, _, _) = CreateService();
        var ex = await Record.ExceptionAsync(() => svc.DeleteAsync("UNKNOWN"));
        Assert.Null(ex);
    }

    // ── Reglement ────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddReglementAsync_NoPayment_EtatIsNonRegle()
    {
        var (svc, db, config) = CreateService();
        await SeedBasicData(db);
        var created = await svc.CreateAsync(MakeFacture(), [MakeLigne("PR00001", 1, 200, tva: 0)], config);

        Assert.Equal("Non Réglé", (await db.FacturesClient.FindAsync(created.NumeroFactureClient))!.EtatReglement);
    }

    [Fact]
    public async Task AddReglementAsync_PartialPayment_SetsPartiellementRegle()
    {
        var (svc, db, config) = CreateService();
        await SeedBasicData(db);
        // TTC = 500, Timbre = 0.6 → totalDu = 500.6
        var created = await svc.CreateAsync(MakeFacture(), [MakeLigne("PR00001", 1, 500, tva: 0)], config);

        await svc.AddReglementAsync(new ReglementFactureClient
        {
            NumeroFactureClient = created.NumeroFactureClient,
            Montant = 100,
            DateReglement = DateTime.Today,
            CodeModePayement = 1
        });

        Assert.Equal("Partiellement Réglé", (await db.FacturesClient.FindAsync(created.NumeroFactureClient))!.EtatReglement);
    }

    [Fact]
    public async Task AddReglementAsync_FullPayment_SetsRegle()
    {
        var (svc, db, config) = CreateService();
        await SeedBasicData(db);
        // TTC = 100, Timbre = 0.6 → totalDu = 100.6
        var created = await svc.CreateAsync(MakeFacture(), [MakeLigne("PR00001", 1, 100, tva: 0)], config);

        await svc.AddReglementAsync(new ReglementFactureClient
        {
            NumeroFactureClient = created.NumeroFactureClient,
            Montant = 100.6,
            DateReglement = DateTime.Today,
            CodeModePayement = 1
        });

        Assert.Equal("Réglé", (await db.FacturesClient.FindAsync(created.NumeroFactureClient))!.EtatReglement);
    }

    [Fact]
    public async Task AddReglementAsync_OverPayment_SetsRegle()
    {
        var (svc, db, config) = CreateService();
        await SeedBasicData(db);
        var created = await svc.CreateAsync(MakeFacture(), [MakeLigne("PR00001", 1, 100, tva: 0)], config);

        await svc.AddReglementAsync(new ReglementFactureClient
        {
            NumeroFactureClient = created.NumeroFactureClient,
            Montant = 200,          // more than totalDu
            DateReglement = DateTime.Today,
            CodeModePayement = 1
        });

        Assert.Equal("Réglé", (await db.FacturesClient.FindAsync(created.NumeroFactureClient))!.EtatReglement);
    }

    // ── DeleteReglement ──────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteReglementAsync_RemovesReglementAndRecomputesEtat()
    {
        var (svc, db, config) = CreateService();
        await SeedBasicData(db);
        // TTC = 100, Timbre = 0.6 → totalDu = 100.6
        var created = await svc.CreateAsync(MakeFacture(), [MakeLigne("PR00001", 1, 100, tva: 0)], config);
        await svc.AddReglementAsync(new ReglementFactureClient
        {
            NumeroFactureClient = created.NumeroFactureClient,
            Montant = 100.6,
            DateReglement = DateTime.Today,
            CodeModePayement = 1
        });
        var reglementId = (await db.ReglementsFactureClient.FirstAsync()).Id;
        Assert.Equal("Réglé", (await db.FacturesClient.FindAsync(created.NumeroFactureClient))!.EtatReglement);

        await svc.DeleteReglementAsync(reglementId);

        Assert.Equal(0, await db.ReglementsFactureClient.CountAsync());
        Assert.Equal("Non Réglé", (await db.FacturesClient.FindAsync(created.NumeroFactureClient))!.EtatReglement);
    }

    [Fact]
    public async Task DeleteReglementAsync_UnknownId_Throws()
    {
        var (svc, _, _) = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.DeleteReglementAsync(9999));
    }

    // ── GetSolde ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetSoldeAsync_NoPaiement_ReturnsMontantTTCPlusTimbre()
    {
        var (svc, db, config) = CreateService();
        await SeedBasicData(db);
        // TTC = 200, Timbre = 0.6 → solde = 200.6
        var created = await svc.CreateAsync(MakeFacture(), [MakeLigne("PR00001", 1, 200, tva: 0)], config);

        var solde = await svc.GetSoldeAsync(created.NumeroFactureClient);

        Assert.Equal(200.6, solde);
    }

    [Fact]
    public async Task GetSoldeAsync_AfterPartialPayment_ReturnsRemainder()
    {
        var (svc, db, config) = CreateService();
        await SeedBasicData(db);
        // TTC = 200, Timbre = 0.6 → totalDu = 200.6; paid = 50 → solde = 150.6
        var created = await svc.CreateAsync(MakeFacture(), [MakeLigne("PR00001", 1, 200, tva: 0)], config);
        await svc.AddReglementAsync(new ReglementFactureClient
        {
            NumeroFactureClient = created.NumeroFactureClient,
            Montant = 50,
            DateReglement = DateTime.Today,
            CodeModePayement = 1
        });

        var solde = await svc.GetSoldeAsync(created.NumeroFactureClient);

        Assert.Equal(150.6, solde);
    }

    [Fact]
    public async Task GetSoldeAsync_UnknownNumero_ReturnsZero()
    {
        var (svc, _, _) = CreateService();
        Assert.Equal(0, await svc.GetSoldeAsync("UNKNOWN"));
    }

    // ── Update ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ReplacesLignesAndAdjustsStock()
    {
        var (svc, db, config) = CreateService();
        await SeedBasicData(db);
        db.Produits.Add(new Produit
        {
            CodeProduit = "PR00002", DesignationProduit = "Second",
            CodeDevise = 1, CodeUniteProduit = 1, CodeTvaProduit = 1,
            CodeCategorieProduit = 1, CodeFabriquantProduit = 1,
            Quantite = 50, StockMinimal = 5
        });
        await db.SaveChangesAsync();

        // Create with PR00001 x5 → stock PR00001 = 95
        var created = await svc.CreateAsync(MakeFacture(), [MakeLigne("PR00001", 5, 100)], config);
        Assert.Equal(95, (await db.Produits.FindAsync("PR00001"))!.Quantite);

        // Update to PR00002 x3 → stock PR00001 restored=100, PR00002 decreased=47
        await svc.UpdateAsync(created, [MakeLigne("PR00002", 3, 80)]);

        Assert.Equal(100, (await db.Produits.FindAsync("PR00001"))!.Quantite);
        Assert.Equal(47,  (await db.Produits.FindAsync("PR00002"))!.Quantite);
        // Old line gone, new line saved
        Assert.Equal(1, await db.LignesFactureClient.CountAsync());
        Assert.Equal("PR00002", (await db.LignesFactureClient.FirstAsync()).CodeProduit);
    }
}
