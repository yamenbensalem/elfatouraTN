using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;
using Web_T4C_GestCom.Tests.Helpers;
using Xunit;

namespace Web_T4C_GestCom.Tests.Services;

public class DevisClientServiceTests
{
    [Fact]
    public async Task CreateAsync_LineWithNegativeQuantite_Throws()
    {
        var db = DbContextFactory.Create();
        db.Clients.Add(new Client { CodeClient = "CL00001", NomClient = "Client Test", CodeDevise = 1 });
        await db.SaveChangesAsync();

        var service = new DevisClientService(db, new DocumentNumberService(db), new NoOpJournalActiviteService());
        var devis = new DevisClient { CodeClient = "CL00001", DateDevis = DateTime.Today };
        var lignes = new List<LigneDevisClient>
        {
            new() { CodeProduit = "PR00001", Quantite = -1, PrixUnitaire = 10, MontantHT = -10 }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(devis, lignes, new AppConfigService(new ConfigurationBuilder().Build())));
    }

    [Fact]
    public async Task UpdateAsync_ReplacesPreviousLines_InsteadOfKeepingOldOnes()
    {
        var db = DbContextFactory.Create();

        db.Clients.Add(new Client
        {
            CodeClient = "CL00001",
            NomClient = "Client Test",
            CodeDevise = 1
        });
        await db.SaveChangesAsync();

        var devis = new DevisClient
        {
            NumeroDevis = "DV202604001",
            DateDevis = DateTime.Today,
            CodeClient = "CL00001",
            EtatDevis = "Ouvert"
        };

        db.DevisClient.Add(devis);
        db.LignesDevisClient.Add(new LigneDevisClient
        {
            NumeroDevis = devis.NumeroDevis,
            CodeProduit = "PR-OLD",
            Quantite = 1,
            PrixUnitaire = 10,
            Remise = 0,
            Tva = 19,
            MontantHT = 10
        });
        await db.SaveChangesAsync();

        var service = new DevisClientService(db, new DocumentNumberService(db), new NoOpJournalActiviteService());

        var updated = new DevisClient
        {
            NumeroDevis = devis.NumeroDevis,
            DateDevis = DateTime.Today,
            CodeClient = "CL00001",
            Remise = 0,
            Timbre = 0,
            EtatDevis = "Ouvert"
        };

        var updatedLines = new List<LigneDevisClient>
        {
            new()
            {
                NumeroDevis = devis.NumeroDevis,
                CodeProduit = "PR-NEW",
                Quantite = 2,
                PrixUnitaire = 20,
                Remise = 0,
                Tva = 19,
                MontantHT = 40
            }
        };

        await service.UpdateAsync(updated, updatedLines);

        var linesInDb = await db.LignesDevisClient
            .Where(l => l.NumeroDevis == devis.NumeroDevis)
            .ToListAsync();

        Assert.Single(linesInDb);
        Assert.Equal("PR-NEW", linesInDb[0].CodeProduit);
    }

    [Fact]
    public async Task CloneAsync_AfterUpdate_UsesOnlyCurrentLines()
    {
        var db = DbContextFactory.Create();

        db.Clients.Add(new Client
        {
            CodeClient = "CL00001",
            NomClient = "Client Test",
            CodeDevise = 1
        });
        await db.SaveChangesAsync();

        var devis = new DevisClient
        {
            NumeroDevis = "DV202604010",
            DateDevis = DateTime.Today,
            CodeClient = "CL00001",
            EtatDevis = "Ouvert"
        };

        db.DevisClient.Add(devis);
        db.LignesDevisClient.Add(new LigneDevisClient
        {
            NumeroDevis = devis.NumeroDevis,
            CodeProduit = "PR-OLD",
            Quantite = 1,
            PrixUnitaire = 10,
            Remise = 0,
            Tva = 19,
            MontantHT = 10
        });
        await db.SaveChangesAsync();

        var service = new DevisClientService(db, new DocumentNumberService(db), new NoOpJournalActiviteService());

        await service.UpdateAsync(
            new DevisClient
            {
                NumeroDevis = devis.NumeroDevis,
                DateDevis = DateTime.Today,
                CodeClient = "CL00001",
                Remise = 0,
                Timbre = 0,
                EtatDevis = "Ouvert"
            },
            [
                new LigneDevisClient
                {
                    NumeroDevis = devis.NumeroDevis,
                    CodeProduit = "PR-NEW",
                    Quantite = 3,
                    PrixUnitaire = 15,
                    Remise = 0,
                    Tva = 19,
                    MontantHT = 45
                }
            ]
        );

        var clone = await service.CloneAsync(devis.NumeroDevis);

        var cloneLines = await db.LignesDevisClient
            .Where(l => l.NumeroDevis == clone.NumeroDevis)
            .ToListAsync();

        Assert.Single(cloneLines);
        Assert.Equal("PR-NEW", cloneLines[0].CodeProduit);
    }

    [Fact]
    public async Task CreateAsync_RecordsJournalEntry()
    {
        var db = DbContextFactory.Create();
        db.Clients.Add(new Client { CodeClient = "CL00001", NomClient = "Client Test", CodeDevise = 1 });
        await db.SaveChangesAsync();

        var journal = new JournalActiviteService(db, new StubCurrentUserService("jdoe"));
        var service = new DevisClientService(db, new DocumentNumberService(db), journal);

        var devis = await service.CreateAsync(
            new DevisClient { CodeClient = "CL00001", DateDevis = DateTime.Today },
            [],
            new AppConfigService(new ConfigurationBuilder().Build()));

        var entries = await journal.GetAllAsync(entite: "Devis");
        Assert.Single(entries);
        Assert.Equal("Ajout", entries[0].Action);
        Assert.Equal(devis.NumeroDevis, entries[0].CodeEntite);
        Assert.Equal("jdoe", entries[0].Login);
    }
}
