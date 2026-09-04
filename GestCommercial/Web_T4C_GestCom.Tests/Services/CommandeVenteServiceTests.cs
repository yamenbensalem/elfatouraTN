using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;
using Web_T4C_GestCom.Tests.Helpers;
using Xunit;

namespace Web_T4C_GestCom.Tests.Services;

public class CommandeVenteServiceTests
{
    private static (CommandeVenteService svc, AppDbContext db) CreateService()
    {
        var db = DbContextFactory.Create();
        return (new CommandeVenteService(db, new DocumentNumberService(db), new NoOpJournalActiviteService()), db);
    }

    private static async Task SeedBasicData(AppDbContext db)
    {
        db.Clients.Add(new Client { CodeClient = "CL00001", NomClient = "Test Client", CodeDevise = 1 });
        await db.SaveChangesAsync();
    }

    private static CommandeVente MakeCommande(string codeClient = "CL00001")
        => new() { CodeClient = codeClient, DateCommandeVente = DateTime.Today };

    private static LigneCommandeVente MakeLigne(string codeProduit, double qte, double pu, double tva = 19)
        => new() { CodeProduit = codeProduit, Quantite = qte, PrixUnitaire = pu, MontantHT = qte * pu, Tva = tva };

    [Fact]
    public async Task CreateAsync_AssignsAutoNumeroAndPersists()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);

        var result = await svc.CreateAsync(MakeCommande(), [MakeLigne("PR00001", 2, 100)]);

        Assert.StartsWith("CV", result.NumeroCommandeVente);
        Assert.Equal(1, await db.CommandesVente.CountAsync());
        Assert.Equal(1, await db.LignesCommandeVente.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_LineWithNegativePrixUnitaire_Throws()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.CreateAsync(MakeCommande(), [MakeLigne("PR00001", 2, -100)]));
    }

    [Fact]
    public async Task CreateAsync_RecalculatesTotalsCorrectly()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);
        var commande = MakeCommande(); commande.Remise = 10;
        var ligne = MakeLigne("PR00001", 2, 100, tva: 19);

        var result = await svc.CreateAsync(commande, [ligne]);

        Assert.Equal(200, result.MontantHT);
        Assert.Equal(38, result.MontantTVA);
        Assert.Equal(218, result.MontantTTC); // 200 - 20(remise) + 38(tva)
    }

    [Fact]
    public async Task UpdateAsync_ReplacesLignesInsteadOfKeepingOldOnes()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);
        var created = await svc.CreateAsync(MakeCommande(), [MakeLigne("PR-OLD", 1, 10)]);

        await svc.UpdateAsync(created, [MakeLigne("PR-NEW", 2, 20)]);

        var lignes = await db.LignesCommandeVente.Where(l => l.NumeroCommandeVente == created.NumeroCommandeVente).ToListAsync();
        Assert.Single(lignes);
        Assert.Equal("PR-NEW", lignes[0].CodeProduit);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCommandeAndLines()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);
        var created = await svc.CreateAsync(MakeCommande(), [MakeLigne("PR00001", 1, 10)]);

        await svc.DeleteAsync(created.NumeroCommandeVente);

        Assert.Equal(0, await db.CommandesVente.CountAsync());
        Assert.Equal(0, await db.LignesCommandeVente.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_UnknownNumero_DoesNotThrow()
    {
        var (svc, _) = CreateService();
        var ex = await Record.ExceptionAsync(() => svc.DeleteAsync("UNKNOWN"));
        Assert.Null(ex);
    }

    [Fact]
    public async Task CloneAsync_AfterUpdate_UsesOnlyCurrentLines()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);
        var created = await svc.CreateAsync(MakeCommande(), [MakeLigne("PR-OLD", 1, 10)]);
        await svc.UpdateAsync(created, [MakeLigne("PR-NEW", 3, 15)]);

        var clone = await svc.CloneAsync(created.NumeroCommandeVente);

        var cloneLines = await db.LignesCommandeVente.Where(l => l.NumeroCommandeVente == clone.NumeroCommandeVente).ToListAsync();
        Assert.Single(cloneLines);
        Assert.Equal("PR-NEW", cloneLines[0].CodeProduit);
    }

    [Fact]
    public async Task GetAllAsync_FilterByClientCode_ReturnsOnlyThatClient()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);
        db.Clients.Add(new Client { CodeClient = "CL00002", NomClient = "Other", CodeDevise = 1 });
        await db.SaveChangesAsync();

        await svc.CreateAsync(MakeCommande("CL00001"), []);
        await svc.CreateAsync(MakeCommande("CL00002"), []);

        var results = await svc.GetAllAsync(clientCode: "CL00001");

        Assert.Single(results);
        Assert.Equal("CL00001", results[0].CodeClient);
    }

    [Fact]
    public async Task CreateAsync_RecordsJournalEntry()
    {
        var db = DbContextFactory.Create();
        await SeedBasicData(db);
        var journal = new JournalActiviteService(db, new StubCurrentUserService("jdoe"));
        var svc = new CommandeVenteService(db, new DocumentNumberService(db), journal);

        var commande = await svc.CreateAsync(MakeCommande(), [MakeLigne("PR00001", 2, 100)]);

        var entries = await journal.GetAllAsync(entite: "CommandeVente");
        Assert.Single(entries);
        Assert.Equal("Ajout", entries[0].Action);
        Assert.Equal(commande.NumeroCommandeVente, entries[0].CodeEntite);
        Assert.Equal("jdoe", entries[0].Login);
    }
}
