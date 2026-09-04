using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;
using Web_T4C_GestCom.Tests.Helpers;
using Xunit;

namespace Web_T4C_GestCom.Tests.Services;

public class CommandeAchatServiceTests
{
    private static (CommandeAchatService svc, AppDbContext db) CreateService()
    {
        var db = DbContextFactory.Create();
        return (new CommandeAchatService(db, new DocumentNumberService(db), new NoOpJournalActiviteService()), db);
    }

    private static async Task SeedBasicData(AppDbContext db)
    {
        db.Fournisseurs.Add(new Fournisseur { CodeFournisseur = "FR00001", NomFournisseur = "Test Fournisseur", CodeDevise = 1 });
        await db.SaveChangesAsync();
    }

    private static CommandeAchat MakeCommande(string codeFournisseur = "FR00001")
        => new() { CodeFournisseur = codeFournisseur, DateCommandeAchat = DateTime.Today };

    private static LigneCommandeAchat MakeLigne(string codeProduit, double qte, double pu, double tva = 19)
        => new() { CodeProduit = codeProduit, Quantite = qte, PrixUnitaire = pu, MontantHT = qte * pu, Tva = tva };

    [Fact]
    public async Task CreateAsync_AssignsAutoNumeroAndPersists()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);

        var result = await svc.CreateAsync(MakeCommande(), [MakeLigne("PR00001", 2, 100)]);

        Assert.StartsWith("CA", result.NumeroCommandeAchat);
        Assert.Equal(1, await db.CommandesAchat.CountAsync());
        Assert.Single(result.Lignes);
    }

    [Fact]
    public async Task CreateAsync_LineWithNegativeQuantite_Throws()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.CreateAsync(MakeCommande(), [MakeLigne("PR00001", -2, 100)]));
    }

    [Fact]
    public async Task CreateAsync_RecalculatesTotalsCorrectly()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);

        var result = await svc.CreateAsync(MakeCommande(), [MakeLigne("PR00001", 2, 100, tva: 19)]);

        Assert.Equal(200, result.MontantHT);
        Assert.Equal(38, result.MontantTVA);
        Assert.Equal(238, result.MontantTTC);
    }

    [Fact]
    public async Task UpdateAsync_ReplacesLignesInsteadOfKeepingOldOnes()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);
        var created = await svc.CreateAsync(MakeCommande(), [MakeLigne("PR-OLD", 1, 10)]);

        await svc.UpdateAsync(created, [MakeLigne("PR-NEW", 2, 20)]);

        var lignes = await db.LignesCommandeAchat.Where(l => l.NumeroCommandeAchat == created.NumeroCommandeAchat).ToListAsync();
        Assert.Single(lignes);
        Assert.Equal("PR-NEW", lignes[0].CodeProduit);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCommandeAndLines()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);
        var created = await svc.CreateAsync(MakeCommande(), [MakeLigne("PR00001", 1, 10)]);

        await svc.DeleteAsync(created.NumeroCommandeAchat);

        Assert.Equal(0, await db.CommandesAchat.CountAsync());
        Assert.Equal(0, await db.LignesCommandeAchat.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_UnknownNumero_Throws()
    {
        var (svc, _) = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.DeleteAsync("UNKNOWN"));
    }

    [Fact]
    public async Task CloneAsync_AfterUpdate_UsesOnlyCurrentLines()
    {
        var (svc, db) = CreateService();
        await SeedBasicData(db);
        var created = await svc.CreateAsync(MakeCommande(), [MakeLigne("PR-OLD", 1, 10)]);
        await svc.UpdateAsync(created, [MakeLigne("PR-NEW", 3, 15)]);

        var clone = await svc.CloneAsync(created.NumeroCommandeAchat);

        var cloneLines = await db.LignesCommandeAchat.Where(l => l.NumeroCommandeAchat == clone.NumeroCommandeAchat).ToListAsync();
        Assert.Single(cloneLines);
        Assert.Equal("PR-NEW", cloneLines[0].CodeProduit);
    }

    [Fact]
    public async Task GetByNumeroAsync_UnknownNumero_ReturnsNull()
    {
        var (svc, _) = CreateService();
        Assert.Null(await svc.GetByNumeroAsync("UNKNOWN"));
    }

    [Fact]
    public async Task CreateAsync_RecordsJournalEntry()
    {
        var db = DbContextFactory.Create();
        await SeedBasicData(db);
        var journal = new JournalActiviteService(db, new StubCurrentUserService("jdoe"));
        var svc = new CommandeAchatService(db, new DocumentNumberService(db), journal);

        var commande = await svc.CreateAsync(MakeCommande(), [MakeLigne("PR00001", 2, 100)]);

        var entries = await journal.GetAllAsync(entite: "CommandeAchat");
        Assert.Single(entries);
        Assert.Equal("Ajout", entries[0].Action);
        Assert.Equal(commande.NumeroCommandeAchat, entries[0].CodeEntite);
        Assert.Equal("jdoe", entries[0].Login);
    }
}
