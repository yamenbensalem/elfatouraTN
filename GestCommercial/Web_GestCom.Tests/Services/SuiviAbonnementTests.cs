using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;
using Web_GestCom.Tests.Helpers;
using Xunit;

namespace Web_GestCom.Tests.Services;

public class SuiviAbonnementTests
{
    private static readonly DateTime Aujourdhui = new(2026, 10, 9);

    private static Abonnement Client(string statut, string plan = "Pro", string cycle = "Annuel", DateTime? debut = null, DateTime? echeance = null) => new()
    {
        NomEntreprise = "Société Exemple", NomContact = "Karim Ben Ali", EmailContact = "karim@exemple.tn",
        Plan = plan, CycleFacturation = cycle, Statut = statut, DateDebut = debut, DateEcheance = echeance,
        PrixCatalogue = 590, PrixApplique = 590
    };

    // ── État de suivi ─────────────────────────────────────────────────────

    [Theory]
    [InlineData("EnAttente")]
    [InlineData("Contactee")]
    [InlineData("Refusee")]
    public void Evaluer_WhenNotActivated_ShouldHaveNothingToFollow(string statut)
    {
        // Act
        var suivi = SuiviAbonnement.Evaluer(Client(statut, echeance: Aujourdhui.AddDays(-5)), Aujourdhui);

        // Assert
        Assert.Equal(EtatSuivi.SansObjet, suivi.Etat);
    }

    [Theory]
    [InlineData("Essai")]
    [InlineData("Active")]
    public void Evaluer_WhenActiveWithoutDueDate_ShouldAskForDates(string statut)
    {
        // Act
        var suivi = SuiviAbonnement.Evaluer(Client(statut), Aujourdhui);

        // Assert — sans échéance, ce client ne remonterait jamais « à relancer ».
        Assert.Equal(EtatSuivi.DatesManquantes, suivi.Etat);
    }

    [Theory]
    [InlineData(31, EtatSuivi.EnCours)]
    [InlineData(30, EtatSuivi.ARelancer)]
    [InlineData(1, EtatSuivi.ARelancer)]
    [InlineData(0, EtatSuivi.ARelancer)]
    [InlineData(-1, EtatSuivi.Expire)]
    public void Evaluer_AnnualSubscription_ShouldBeToRemindThirtyDaysBeforeDueDate(int joursRestants, EtatSuivi attendu)
    {
        // Act
        var suivi = SuiviAbonnement.Evaluer(Client("Active", echeance: Aujourdhui.AddDays(joursRestants)), Aujourdhui);

        // Assert
        Assert.Equal(attendu, suivi.Etat);
        Assert.Equal(joursRestants, suivi.JoursRestants);
    }

    [Theory]
    [InlineData("Active", "Mensuel", 8, EtatSuivi.EnCours)]
    [InlineData("Active", "Mensuel", 7, EtatSuivi.ARelancer)]
    [InlineData("Essai", "Annuel", 8, EtatSuivi.EnCours)]
    [InlineData("Essai", "Annuel", 7, EtatSuivi.ARelancer)]
    public void Evaluer_ShortPeriods_ShouldBeToRemindOneWeekBefore(string statut, string cycle, int joursRestants, EtatSuivi attendu)
    {
        // Act
        var suivi = SuiviAbonnement.Evaluer(Client(statut, cycle: cycle, echeance: Aujourdhui.AddDays(joursRestants)), Aujourdhui);

        // Assert
        Assert.Equal(attendu, suivi.Etat);
    }

    [Fact]
    public void Evaluer_DesktopLicenceWithStartButNoDueDate_ShouldHaveNothingToFollow()
    {
        // Arrange — licence livrée, sans maintenance à suivre : elle ne se termine pas.
        var licence = Client("Active", plan: TarifsOptions.PlanDesktopEssentiel, debut: Aujourdhui.AddMonths(-6));

        // Act + Assert
        Assert.Equal(EtatSuivi.SansObjet, SuiviAbonnement.Evaluer(licence, Aujourdhui).Etat);
    }

    [Fact]
    public void Evaluer_DesktopMaintenance_ShouldUseTheThirtyDayThresholdEvenIfCycleSaysMonthly()
    {
        // Arrange — le cycle ne s'applique pas à un achat unique.
        var licence = Client("Active", plan: TarifsOptions.PlanDesktopSerenite, cycle: "Mensuel", echeance: Aujourdhui.AddDays(20));

        // Act + Assert
        Assert.Equal(EtatSuivi.ARelancer, SuiviAbonnement.Evaluer(licence, Aujourdhui).Etat);
    }

    // ── Dates proposées à l'activation ────────────────────────────────────

    [Theory]
    [InlineData("Essai", "Standard", "Annuel", "2026-11-08")]            // 30 jours d'essai
    [InlineData("Active", "Pro", "Annuel", "2027-10-09")]                // 1 an
    [InlineData("Active", "Pro", "Mensuel", "2026-11-09")]               // 1 mois
    [InlineData("Active", "Enterprise", "Annuel", "2027-10-09")]         // sur devis : 1 an par défaut
    [InlineData("Active", "DesktopEssentiel", "Annuel", "2026-12-09")]   // garantie 2 mois
    [InlineData("Active", "DesktopSerenite", "Annuel", "2027-10-09")]    // maintenance incluse 12 mois
    [InlineData("Active", "DesktopEquipe", "Annuel", "2027-10-09")]
    public void DatesProposees_ShouldFollowTheOffer(string statut, string plan, string cycle, string echeanceAttendue)
    {
        // Act
        var (debut, echeance) = SuiviAbonnement.DatesProposees(Client(statut, plan, cycle), new TarifsOptions(), Aujourdhui);

        // Assert
        Assert.Equal(Aujourdhui, debut);
        Assert.Equal(DateTime.Parse(echeanceAttendue), echeance);
    }

    // ── Relance par email ─────────────────────────────────────────────────

    private sealed class FakeTransport(bool accepte) : IEmailTransport
    {
        public List<(string To, string Subject, string Html)> Sent { get; } = [];

        public Task<bool> SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken ct = default)
        {
            Sent.Add((toEmail, subject, htmlBody));
            return Task.FromResult(accepte);
        }
    }

    private static AbonnementService CreateService(AppDbContext db, IEmailTransport transport) => new(
        db, transport, Options.Create(new EmailOptions { AdminNotificationEmail = "contact@gestcom.test" }),
        Options.Create(new TarifsOptions()), NullLogger<AbonnementService>.Instance);

    private static async Task<Abonnement> SeedAsync(AppDbContext db, Abonnement abonnement)
    {
        db.Abonnements.Add(abonnement);
        await db.SaveChangesAsync();
        return abonnement;
    }

    [Fact]
    public async Task RelancerAsync_WhenEmailAccepted_ShouldSendDueDateReminderAndRecordTheDate()
    {
        // Arrange
        await using var db = DbContextFactory.Create();
        var transport = new FakeTransport(accepte: true);
        var client = await SeedAsync(db, Client("Active", echeance: DateTime.Today.AddDays(12)));

        // Act
        var envoye = await CreateService(db, transport).RelancerAsync(client.Id);

        // Assert
        Assert.True(envoye);
        var email = Assert.Single(transport.Sent);
        Assert.Equal("karim@exemple.tn", email.To);
        Assert.Contains("arrive à échéance", email.Html);
        Assert.Contains(DateTime.Today.AddDays(12).ToString("dd/MM/yyyy"), email.Html);
        Assert.Contains("590 DT HT / an", email.Html);
        Assert.Contains("contact@gestcom.test", email.Html);
        Assert.NotNull((await db.Abonnements.AsNoTracking().SingleAsync()).DateDerniereRelance);
    }

    [Fact]
    public async Task RelancerAsync_WhenEmailRefused_ShouldReturnFalseAndNotPretendItWasSent()
    {
        // Arrange — Brevo refuse (IP non autorisée, quota...) ou aucun fournisseur n'est configuré.
        await using var db = DbContextFactory.Create();
        var client = await SeedAsync(db, Client("Active", echeance: DateTime.Today.AddDays(12)));

        // Act
        var envoye = await CreateService(db, new FakeTransport(accepte: false)).RelancerAsync(client.Id);

        // Assert
        Assert.False(envoye);
        Assert.Null((await db.Abonnements.AsNoTracking().SingleAsync()).DateDerniereRelance);
    }

    [Fact]
    public async Task RelancerAsync_WhenAlreadyExpired_ShouldSayItIsOver()
    {
        // Arrange
        await using var db = DbContextFactory.Create();
        var transport = new FakeTransport(accepte: true);
        var client = await SeedAsync(db, Client("Active", echeance: DateTime.Today.AddDays(-3)));

        // Act
        await CreateService(db, transport).RelancerAsync(client.Id);

        // Assert
        Assert.Contains("est arrivé", transport.Sent.Single().Html);
    }

    [Fact]
    public async Task RelancerAsync_ForTrial_ShouldInviteToConfirmTheSubscription()
    {
        // Arrange
        await using var db = DbContextFactory.Create();
        var transport = new FakeTransport(accepte: true);
        var client = await SeedAsync(db, Client("Essai", plan: "Standard", echeance: DateTime.Today.AddDays(5)));

        // Act
        await CreateService(db, transport).RelancerAsync(client.Id);

        // Assert
        var email = transport.Sent.Single();
        Assert.Contains("essai gratuit", email.Subject);
        Assert.Contains("confirmer votre abonnement Standard", email.Html);
    }

    [Fact]
    public async Task RelancerAsync_ForDesktopMaintenance_ShouldSayTheLicenceKeepsWorkingAndGiveRenewalPrice()
    {
        // Arrange
        await using var db = DbContextFactory.Create();
        var transport = new FakeTransport(accepte: true);
        var client = await SeedAsync(db, Client("Active", plan: TarifsOptions.PlanDesktopSerenite, echeance: DateTime.Today.AddDays(20)));

        // Act
        await CreateService(db, transport).RelancerAsync(client.Id);

        // Assert
        var email = transport.Sent.Single();
        Assert.Contains("maintenance", email.Subject);
        Assert.Contains("Votre licence continue de fonctionner", email.Html);
        Assert.Contains("350 DT HT", email.Html);
    }

    [Theory]
    [InlineData("EnAttente", true)]
    [InlineData("Active", false)]
    public async Task RelancerAsync_WhenNothingToRemind_ShouldRefuseWithoutSending(string statut, bool avecEcheance)
    {
        // Arrange — demande non activée, ou client actif sans échéance.
        await using var db = DbContextFactory.Create();
        var transport = new FakeTransport(accepte: true);
        var client = await SeedAsync(db, Client(statut, echeance: avecEcheance ? DateTime.Today.AddDays(5) : null));

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService(db, transport).RelancerAsync(client.Id));
        Assert.Empty(transport.Sent);
    }
}
