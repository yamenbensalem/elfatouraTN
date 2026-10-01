using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;
using Web_GestCom.Tests.Helpers;
using Xunit;

namespace Web_GestCom.Tests.Services;

public class AbonnementServiceTests
{
    private static AbonnementService CreateService(out AppDbContext db, IEmailTransport? emailTransport = null)
    {
        db = DbContextFactory.Create();
        return new AbonnementService(
            db,
            emailTransport ?? new NoOpEmailTransport(NullLogger<NoOpEmailTransport>.Instance),
            Options.Create(new EmailOptions { AdminNotificationEmail = "admin@test.com" }),
            Options.Create(new TarifsOptions()),
            NullLogger<AbonnementService>.Instance);
    }

    [Fact]
    public async Task CreateDemandeAsync_StoresRequestAsPending()
    {
        var svc = CreateService(out var db);

        await svc.CreateDemandeAsync(new Abonnement
        {
            NomEntreprise = "Société Test",
            NomContact = "Karim Ben Ali",
            EmailContact = "Karim@Test.com",
            Plan = "Pro"
        });

        var stored = await db.Abonnements.SingleAsync();
        Assert.Equal("EnAttente", stored.Statut);
        Assert.Null(stored.CompanyId);
        Assert.Null(stored.DateDebut);
    }

    [Fact]
    public async Task CreateDemandeAsync_IgnoresCallerSuppliedStatutAndCompanyId()
    {
        var svc = CreateService(out var db);
        var company = new Company { Name = "Alpha" };
        db.Companies.Add(company);
        await db.SaveChangesAsync();

        await svc.CreateDemandeAsync(new Abonnement
        {
            NomEntreprise = "Société Test",
            NomContact = "Karim",
            EmailContact = "karim@test.com",
            Plan = "Standard",
            Statut = "Active",
            CompanyId = company.Id
        });

        var stored = await db.Abonnements.SingleAsync();
        Assert.Equal("EnAttente", stored.Statut);
        Assert.Null(stored.CompanyId);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsRequestsAcrossAllCompanies_OrderedByMostRecent()
    {
        var svc = CreateService(out var db);
        db.Abonnements.AddRange(
            new Abonnement { NomEntreprise = "Ancienne", NomContact = "A", EmailContact = "a@test.com", Plan = "Standard", DateDemande = DateTime.UtcNow.AddDays(-2) },
            new Abonnement { NomEntreprise = "Récente", NomContact = "B", EmailContact = "b@test.com", Plan = "Pro", DateDemande = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var result = await svc.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Récente", result[0].NomEntreprise);
    }

    [Fact]
    public async Task UpdateAsync_ActivatesRequestAndLinksCompany()
    {
        var svc = CreateService(out var db);
        var company = new Company { Name = "Beta" };
        db.Companies.Add(company);
        var demande = new Abonnement { NomEntreprise = "Beta", NomContact = "C", EmailContact = "c@test.com", Plan = "Pro" };
        db.Abonnements.Add(demande);
        await db.SaveChangesAsync();
        db.Entry(demande).State = EntityState.Detached;
        db.Entry(company).State = EntityState.Detached;

        demande.Statut = "Active";
        demande.CompanyId = company.Id;
        demande.DateDebut = DateTime.UtcNow;
        await svc.UpdateAsync(demande);

        var stored = await db.Abonnements.AsNoTracking().SingleAsync();
        Assert.Equal("Active", stored.Statut);
        Assert.Equal(company.Id, stored.CompanyId);
        Assert.NotNull(stored.DateDebut);
    }

    [Fact]
    public async Task CreateDemandeAsync_SendsConfirmationToRequesterAndNotificationToAdmin()
    {
        var emailTransport = new Mock<IEmailTransport>();
        var svc = CreateService(out _, emailTransport.Object);

        await svc.CreateDemandeAsync(new Abonnement
        {
            NomEntreprise = "Société Test",
            NomContact = "Karim Ben Ali",
            EmailContact = "karim@test.com",
            Plan = "Pro"
        });

        emailTransport.Verify(e => e.SendAsync(
            "karim@test.com", "Karim Ben Ali",
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        emailTransport.Verify(e => e.SendAsync(
            "admin@test.com", It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateDemandeAsync_WhenEmailTransportThrows_StillStoresTheRequest()
    {
        var emailTransport = new Mock<IEmailTransport>();
        emailTransport
            .Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Brevo indisponible"));
        var svc = CreateService(out var db, emailTransport.Object);

        var demande = await svc.CreateDemandeAsync(new Abonnement
        {
            NomEntreprise = "Société Test",
            NomContact = "Karim",
            EmailContact = "karim@test.com",
            Plan = "Standard"
        });

        Assert.True(demande.Id > 0);
        Assert.Equal("EnAttente", (await db.Abonnements.SingleAsync()).Statut);
    }

    [Fact]
    public async Task DeleteAsync_RemovesRequest()
    {
        var svc = CreateService(out var db);
        var demande = new Abonnement { NomEntreprise = "Gamma", NomContact = "D", EmailContact = "d@test.com", Plan = "Standard" };
        db.Abonnements.Add(demande);
        await db.SaveChangesAsync();
        db.Entry(demande).State = EntityState.Detached;

        await svc.DeleteAsync(demande);

        Assert.False(await db.Abonnements.AnyAsync(a => a.Id == demande.Id));
    }

    // ── Tarifs : cycle de facturation, prix figé, code promo ──────────────

    private static Abonnement NouvelleDemande(string plan, string cycle, string? codePromo = null) => new()
    {
        NomEntreprise = "Entreprise Test", NomContact = "Contact", EmailContact = "c@test.com",
        Plan = plan, CycleFacturation = cycle, CodePromo = codePromo
    };

    private static void AjouterCodeFondateur(AppDbContext db, int maxUtilisations = 10, DateTime? expiration = null)
    {
        db.CodesPromo.Add(new CodePromo
        {
            Code = "FONDATEUR2026", Libelle = "Client Fondateur", PourcentageReduction = 35,
            MaxUtilisations = maxUtilisations, DateExpiration = expiration, EstPermanent = true
        });
        db.SaveChanges();
    }

    [Theory]
    [InlineData("Standard", "Annuel", 390)]
    [InlineData("Standard", "Mensuel", 45)]
    [InlineData("Pro", "Annuel", 690)]
    [InlineData("Pro", "Mensuel", 79)]
    public async Task CreateDemandeAsync_WhenNoPromo_ShouldStoreCatalogPriceForCycle(string plan, string cycle, double prixAttendu)
    {
        // Arrange
        var svc = CreateService(out _);

        // Act
        var demande = await svc.CreateDemandeAsync(NouvelleDemande(plan, cycle));

        // Assert
        Assert.Equal(cycle, demande.CycleFacturation);
        Assert.Equal(prixAttendu, demande.PrixCatalogue);
        Assert.Equal(prixAttendu, demande.PrixApplique);
        Assert.Null(demande.CodePromo);
    }

    [Fact]
    public async Task CreateDemandeAsync_WhenEnterprise_ShouldStoreNoPrice()
    {
        // Arrange
        var svc = CreateService(out _);

        // Act
        var demande = await svc.CreateDemandeAsync(NouvelleDemande("Enterprise", "Annuel"));

        // Assert
        Assert.Null(demande.PrixCatalogue);
        Assert.Null(demande.PrixApplique);
    }

    [Fact]
    public async Task CreateDemandeAsync_WhenFormSendsItsOwnPrice_ShouldIgnoreItAndRecompute()
    {
        // Arrange
        var svc = CreateService(out _);
        var demande = NouvelleDemande("Pro", "Annuel");
        demande.PrixApplique = 1;
        demande.PrixCatalogue = 1;

        // Act
        var result = await svc.CreateDemandeAsync(demande);

        // Assert
        Assert.Equal(690, result.PrixApplique);
    }

    [Theory]
    [InlineData("Standard", "Annuel", 253.5)]
    [InlineData("Pro", "Mensuel", 51.35)]
    public async Task CreateDemandeAsync_WhenValidPromo_ShouldApplyDiscountAndNormalizeCode(string plan, string cycle, double prixAttendu)
    {
        // Arrange
        var svc = CreateService(out var db);
        AjouterCodeFondateur(db);

        // Act
        var demande = await svc.CreateDemandeAsync(NouvelleDemande(plan, cycle, " fondateur2026 "));

        // Assert
        Assert.Equal("FONDATEUR2026", demande.CodePromo);
        Assert.Equal(prixAttendu, demande.PrixApplique);
    }

    [Fact]
    public async Task CreateDemandeAsync_WhenUnknownPromo_ShouldThrowAndSaveNothing()
    {
        // Arrange
        var svc = CreateService(out var db);

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.CreateDemandeAsync(NouvelleDemande("Pro", "Annuel", "INCONNU")));

        // Assert
        Assert.Equal(AbonnementService.CodePromoInvalideMessage, ex.Message);
        Assert.Empty(db.Abonnements);
    }

    [Fact]
    public async Task GetOffrePromoAsync_WhenExpired_ShouldReturnNull()
    {
        // Arrange
        var svc = CreateService(out var db);
        AjouterCodeFondateur(db, expiration: DateTime.UtcNow.AddDays(-1));

        // Act
        var offre = await svc.GetOffrePromoAsync("FONDATEUR2026");

        // Assert
        Assert.Null(offre);
    }

    [Fact]
    public async Task GetOffrePromoAsync_ShouldCountOnlyActivatedSubscriptionsAsUsedPlaces()
    {
        // Arrange — 4 demandes avec le code : seules Essai et Active consomment une place.
        var svc = CreateService(out var db);
        AjouterCodeFondateur(db, maxUtilisations: 10);
        foreach (var statut in new[] { "EnAttente", "Essai", "Active", "Refusee" })
        {
            var demande = await svc.CreateDemandeAsync(NouvelleDemande("Standard", "Annuel", "FONDATEUR2026"));
            demande.Statut = statut;
        }
        await db.SaveChangesAsync();

        // Act
        var offre = await svc.GetOffrePromoAsync("FONDATEUR2026");

        // Assert
        Assert.NotNull(offre);
        Assert.Equal(8, offre.PlacesRestantes);
    }

    [Fact]
    public async Task GetOffrePromoAsync_WhenAllPlacesTaken_ShouldReturnNull()
    {
        // Arrange
        var svc = CreateService(out var db);
        AjouterCodeFondateur(db, maxUtilisations: 1);
        var demande = await svc.CreateDemandeAsync(NouvelleDemande("Standard", "Annuel", "FONDATEUR2026"));
        demande.Statut = "Active";
        await db.SaveChangesAsync();

        // Act
        var offre = await svc.GetOffrePromoAsync("FONDATEUR2026");

        // Assert
        Assert.Null(offre);
    }
}
