using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Web_GestCom.Auth;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;
using Web_GestCom.Tests.Helpers;
using Xunit;

namespace Web_GestCom.Tests.Services;

/// <summary>
/// Règle décidée le 2026-10-09 : passé l'échéance, 14 jours d'accès maintenu, puis blocage complet
/// jusqu'au règlement ; un essai gratuit est bloqué dès sa fin ; jamais de blocage sans date
/// d'échéance ; les licences Desktop ne sont pas concernées.
/// </summary>
public class AccesEntrepriseTests
{
    private static readonly DateTime Aujourdhui = new(2026, 10, 9);

    private static Abonnement Abo(string statut, int? echeanceDansJours, string plan = "Pro", int companyId = 2) => new()
    {
        NomEntreprise = "Société Exemple", NomContact = "Contact", EmailContact = "c@exemple.tn",
        Plan = plan, Statut = statut, CompanyId = companyId,
        DateEcheance = echeanceDansJours.HasValue ? Aujourdhui.AddDays(echeanceDansJours.Value) : null
    };

    // ── La règle ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(30, EtatAcces.AJour)]
    [InlineData(0, EtatAcces.AJour)]       // le jour de l'échéance est encore couvert
    [InlineData(-1, EtatAcces.EnGrace)]
    [InlineData(-14, EtatAcces.EnGrace)]   // dernier jour d'accès
    [InlineData(-15, EtatAcces.Suspendu)]
    [InlineData(-200, EtatAcces.Suspendu)]
    public void Evaluer_PaidSubscription_ShouldKeepAccessFourteenDaysAfterDueDateThenSuspend(int echeanceDansJours, EtatAcces attendu)
    {
        // Act
        var acces = AccesEntreprise.Evaluer([Abo("Active", echeanceDansJours)], Aujourdhui);

        // Assert
        Assert.Equal(attendu, acces.Etat);
    }

    [Theory]
    [InlineData(0, EtatAcces.AJour)]
    [InlineData(-1, EtatAcces.Suspendu)]   // aucun délai de grâce pour un essai gratuit
    public void Evaluer_Trial_ShouldSuspendAsSoonAsItEnds(int echeanceDansJours, EtatAcces attendu)
    {
        // Act
        var acces = AccesEntreprise.Evaluer([Abo("Essai", echeanceDansJours)], Aujourdhui);

        // Assert
        Assert.Equal(attendu, acces.Etat);
        Assert.True(acces.EstEssai);
    }

    [Fact]
    public void Evaluer_DuringGrace_ShouldTellHowManyDaysRemainBeforeSuspension()
    {
        // Act
        var acces = AccesEntreprise.Evaluer([Abo("Active", -4)], Aujourdhui);

        // Assert
        Assert.Equal(Aujourdhui.AddDays(-4), acces.DateEcheance);
        Assert.Equal(Aujourdhui.AddDays(10), acces.DernierJourAcces);
        Assert.Equal(10, acces.JoursAvantSuspension(Aujourdhui));
    }

    [Fact]
    public void Evaluer_WhenActiveCustomerHasNoDueDate_ShouldNeverSuspend()
    {
        // Arrange — cas de tous les clients déjà actifs avant cette règle.
        var acces = AccesEntreprise.Evaluer([Abo("Active", null)], Aujourdhui);

        // Assert
        Assert.Equal(EtatAcces.Libre, acces.Etat);
    }

    [Fact]
    public void Evaluer_WhenCompanyHasNoSubscriptionAtAll_ShouldNeverSuspend()
    {
        // Act + Assert — entreprise créée à la main par le SuperAdmin, sans demande d'abonnement.
        Assert.Equal(EtatAcces.Libre, AccesEntreprise.Evaluer([], Aujourdhui).Etat);
    }

    [Theory]
    [InlineData("EnAttente")]
    [InlineData("Contactee")]
    [InlineData("Refusee")]
    public void Evaluer_ShouldIgnoreRequestsThatAreNotActive(string statut)
    {
        // Act + Assert
        Assert.Equal(EtatAcces.Libre, AccesEntreprise.Evaluer([Abo(statut, -100)], Aujourdhui).Etat);
    }

    [Fact]
    public void Evaluer_DesktopLicence_ShouldNeverSuspendEvenLongAfterMaintenanceEnded()
    {
        // Act
        var acces = AccesEntreprise.Evaluer([Abo("Active", -300, plan: TarifsOptions.PlanDesktopSerenite)], Aujourdhui);

        // Assert
        Assert.Equal(EtatAcces.Libre, acces.Etat);
    }

    [Fact]
    public void Evaluer_WhenRenewedAlongsideAnOldExpiredLine_ShouldUseTheMostFavourableOne()
    {
        // Act
        var acces = AccesEntreprise.Evaluer([Abo("Active", -90), Abo("Active", 200)], Aujourdhui);

        // Assert
        Assert.Equal(EtatAcces.AJour, acces.Etat);
    }

    [Fact]
    public async Task ChargerAsync_ShouldOnlyLookAtTheCompanyOwnSubscriptions()
    {
        // Arrange — l'entreprise 2 est très en retard, l'entreprise 3 est à jour.
        await using var db = DbContextFactory.Create();
        var retard = Abo("Active", null, companyId: 2);
        retard.DateEcheance = DateTime.Today.AddDays(-60);
        var aJour = Abo("Active", null, companyId: 3);
        aJour.DateEcheance = DateTime.Today.AddDays(60);
        db.Abonnements.AddRange(retard, aJour);
        await db.SaveChangesAsync();

        // Act + Assert
        Assert.Equal(EtatAcces.Suspendu, (await AccesEntreprise.ChargerAsync(db, 2, DateTime.Today)).Etat);
        Assert.Equal(EtatAcces.AJour, (await AccesEntreprise.ChargerAsync(db, 3, DateTime.Today)).Etat);
        Assert.Equal(EtatAcces.Libre, (await AccesEntreprise.ChargerAsync(db, 4, DateTime.Today)).Etat);
    }

    // ── Suivi côté SuperAdmin ─────────────────────────────────────────────

    [Theory]
    [InlineData("Active", "Pro", -14, EtatSuivi.Expire)]
    [InlineData("Active", "Pro", -15, EtatSuivi.Suspendu)]
    [InlineData("Essai", "Standard", -1, EtatSuivi.Suspendu)]
    [InlineData("Active", "DesktopSerenite", -100, EtatSuivi.Expire)]
    public void Suivi_ShouldReflectSuspension(string statut, string plan, int echeanceDansJours, EtatSuivi attendu)
    {
        // Act
        var suivi = SuiviAbonnement.Evaluer(Abo(statut, echeanceDansJours, plan), Aujourdhui);

        // Assert
        Assert.Equal(attendu, suivi.Etat);
    }

    // ── Blocage des requêtes ──────────────────────────────────────────────

    private static async Task<(int Status, string? Location, bool Passe)> AppelerAsync(
        AppDbContext db, string path, int? companyId, bool superAdmin = false, string method = "GET", bool authentifie = true)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, "user"), new("IsSuperAdmin", superAdmin ? "1" : "0") };
        if (companyId.HasValue) claims.Add(new Claim("CompanyId", companyId.Value.ToString()));
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(authentifie ? new ClaimsIdentity(claims, "Tests") : new ClaimsIdentity())
        };
        context.Request.Path = path;
        context.Request.Method = method;

        var passe = false;
        await new SuspensionAccesMiddleware(_ => { passe = true; return Task.CompletedTask; }).InvokeAsync(context, db);
        return (context.Response.StatusCode, context.Response.Headers.Location.ToString(), passe);
    }

    private static async Task<AppDbContext> BaseAvecEntrepriseAsync(int echeanceDansJours, string statut = "Active")
    {
        var db = DbContextFactory.Create();
        var abonnement = Abo(statut, null);
        abonnement.DateEcheance = DateTime.Today.AddDays(echeanceDansJours);
        db.Abonnements.Add(abonnement);
        await db.SaveChangesAsync();
        return db;
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/clients")]
    [InlineData("/factures-client/FC202610001")]
    [InlineData("/print/facture/FC202610001")]
    public async Task Middleware_WhenCompanySuspended_ShouldRedirectEveryPageToTheSuspensionPage(string path)
    {
        // Arrange
        await using var db = await BaseAvecEntrepriseAsync(echeanceDansJours: -30);

        // Act
        var (status, location, passe) = await AppelerAsync(db, path, companyId: 2);

        // Assert — blocage complet : aucune page métier, même en lecture.
        Assert.False(passe);
        Assert.Equal(StatusCodes.Status302Found, status);
        Assert.Equal(SuspensionAccesMiddleware.PageSuspension, location);
    }

    [Theory]
    [InlineData("/_blazor/negotiate", "POST")]
    [InlineData("/_blazor", "GET")]
    [InlineData("/clients", "POST")]
    public async Task Middleware_WhenCompanySuspended_ShouldRefuseBlazorConnectionsAndPosts(string path, string method)
    {
        // Arrange
        await using var db = await BaseAvecEntrepriseAsync(echeanceDansJours: -30);

        // Act
        var (status, _, passe) = await AppelerAsync(db, path, companyId: 2, method: method);

        // Assert
        Assert.False(passe);
        Assert.Equal(StatusCodes.Status403Forbidden, status);
    }

    [Theory]
    [InlineData("/compte/acces-suspendu")]
    [InlineData("/compte/deconnexion")]
    [InlineData("/compte/connexion")]
    [InlineData("/demande-abonnement")]
    [InlineData("/app.css")]
    public async Task Middleware_WhenCompanySuspended_ShouldStillAllowSignOutAndTheExplanationPage(string path)
    {
        // Arrange
        await using var db = await BaseAvecEntrepriseAsync(echeanceDansJours: -30);

        // Act
        var (_, _, passe) = await AppelerAsync(db, path, companyId: 2);

        // Assert
        Assert.True(passe);
    }

    [Theory]
    [InlineData(-14)]  // dernier jour du délai de grâce
    [InlineData(-1)]
    [InlineData(20)]
    public async Task Middleware_WhenNotYetSuspended_ShouldLetTheCustomerIn(int echeanceDansJours)
    {
        // Arrange
        await using var db = await BaseAvecEntrepriseAsync(echeanceDansJours);

        // Act
        var (_, _, passe) = await AppelerAsync(db, "/clients", companyId: 2);

        // Assert
        Assert.True(passe);
    }

    [Fact]
    public async Task Middleware_WhenTrialEnded_ShouldBlockTheNextDay()
    {
        // Arrange
        await using var db = await BaseAvecEntrepriseAsync(echeanceDansJours: -1, statut: "Essai");

        // Act
        var (_, location, passe) = await AppelerAsync(db, "/clients", companyId: 2);

        // Assert
        Assert.False(passe);
        Assert.Equal(SuspensionAccesMiddleware.PageSuspension, location);
    }

    [Fact]
    public async Task Middleware_ShouldNeverBlockSuperAdminOtherCompaniesOrVisitors()
    {
        // Arrange
        await using var db = await BaseAvecEntrepriseAsync(echeanceDansJours: -30);

        // Act + Assert
        Assert.True((await AppelerAsync(db, "/admin/demandes-abonnement", companyId: null, superAdmin: true)).Passe);
        Assert.True((await AppelerAsync(db, "/clients", companyId: 3)).Passe);
        Assert.True((await AppelerAsync(db, "/", companyId: null, authentifie: false)).Passe);
    }

    // ── Blocage des écritures (circuit Blazor resté ouvert) ───────────────

    private static AppDbContext ContexteEntreprise(DbContextOptions<AppDbContext> options, int companyId)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, "admin2"), new Claim("IsSuperAdmin", "0"), new Claim("CompanyId", companyId.ToString())], "Tests"));
        var context = new AppDbContext(options, new HttpExecutionContext(new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = principal } }));
        context.Database.EnsureCreated();
        return context;
    }

    private static async Task<DbContextOptions<AppDbContext>> OptionsAvecEcheanceAsync(int echeanceDansJours)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var seed = new AppDbContext(options);
        var abonnement = Abo("Active", null);
        abonnement.DateEcheance = DateTime.Today.AddDays(echeanceDansJours);
        seed.Abonnements.Add(abonnement);
        await seed.SaveChangesAsync();
        return options;
    }

    [Fact]
    public async Task Services_WhenCompanySuspended_ShouldRefuseWritesEvenForItsAdmin()
    {
        // Arrange — l'Admin contourne les permissions métier, pas la suspension.
        var options = await OptionsAvecEcheanceAsync(echeanceDansJours: -30);
        await using var db = ContexteEntreprise(options, companyId: 2);
        var svc = new ClientService(db, new NoOpJournalActiviteService(), new StubCurrentUserService("admin2", isAdmin: true), new Moq.Mock<IPermissionService>().Object);

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.AddAsync(new Client { NomClient = "Client refusé", CodeDevise = 1 }));

        // Assert
        Assert.StartsWith("Accès suspendu", ex.Message);
        Assert.Equal(ex.Message, UserErrorMessage.Build(ex, "REF"));
        await using var verif = new AppDbContext(options);
        Assert.Empty(verif.Clients);
    }

    [Fact]
    public async Task Services_DuringGrace_ShouldStillAcceptWrites()
    {
        // Arrange
        var options = await OptionsAvecEcheanceAsync(echeanceDansJours: -5);
        await using var db = ContexteEntreprise(options, companyId: 2);
        var svc = new ClientService(db, new NoOpJournalActiviteService(), new StubCurrentUserService("admin2", isAdmin: true), new Moq.Mock<IPermissionService>().Object);

        // Act
        await svc.AddAsync(new Client { NomClient = "Client accepté", CodeDevise = 1 });

        // Assert
        Assert.Equal(1, await db.Clients.CountAsync());
    }
}
