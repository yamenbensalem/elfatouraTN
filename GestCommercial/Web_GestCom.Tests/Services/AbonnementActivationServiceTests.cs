using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;
using Web_GestCom.Tests.Helpers;
using Xunit;

namespace Web_GestCom.Tests.Services;

public class AbonnementActivationServiceTests
{
    private sealed class CapturingEmailTransport : IEmailTransport
    {
        public List<(string To, string Subject, string Html)> Sent { get; } = [];

        public Task<bool> SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken ct = default)
        {
            Sent.Add((toEmail, subject, htmlBody));
            return Task.FromResult(true);
        }
    }

    private static AbonnementActivationService CreateService(out AppDbContext db, out CapturingEmailTransport emails)
    {
        db = DbContextFactory.Create();
        emails = new CapturingEmailTransport();
        var emailOptions = Options.Create(new EmailOptions());
        var superAdmin = new StubCurrentUserService("superadmin", isSuperAdmin: true);
        var abonnements = new AbonnementService(db, new NoOpEmailTransport(NullLogger<NoOpEmailTransport>.Instance),
            emailOptions, Options.Create(new TarifsOptions()), NullLogger<AbonnementService>.Instance);
        return new AbonnementActivationService(
            db, abonnements, new CompanyService(db),
            new UtilisateurService(db, new StubTenantService(), currentUser: superAdmin),
            emails, emailOptions, NullLogger<AbonnementActivationService>.Instance);
    }

    private static Abonnement AjouterDemande(AppDbContext db, string statut = "EnAttente", int? companyId = null)
    {
        var demande = new Abonnement
        {
            NomEntreprise = "Société Exemple", NomContact = "Karim Ben Ali", EmailContact = "karim@exemple.tn",
            Plan = "Pro", Statut = statut, CompanyId = companyId
        };
        db.Abonnements.Add(demande);
        db.SaveChanges();
        return demande;
    }

    private static readonly AccesCompte Acces = new("karim.ben.ali", "Motdepasse9");

    [Theory]
    [InlineData("BEN SALEM", "ben.salem")]
    [InlineData("  Éloïse  d'Été ", "eloise.d.ete")]
    [InlineData("***", "client")]
    public void LoginDepuisNom_ShouldBuildLowercaseDottedLoginWithoutAccents(string nom, string attendu)
    {
        // Act
        var login = AbonnementActivationService.LoginDepuisNom(nom);

        // Assert
        Assert.Equal(attendu, login);
    }

    [Fact]
    public async Task SuggererLoginAsync_WhenLoginTaken_ShouldAppendNumber()
    {
        // Arrange
        var svc = CreateService(out var db, out _);
        db.Utilisateurs.Add(new Utilisateur { Login = "ben.salem", Prenom = "A", Nom = "B", PasswordHash = "x" });
        db.SaveChanges();

        // Act
        var login = await svc.SuggererLoginAsync("Ben Salem");

        // Assert
        Assert.Equal("ben.salem2", login);
    }

    [Fact]
    public void GenererMotDePasse_ShouldBeLongEnoughAndDifferentEachTime()
    {
        // Arrange
        var svc = CreateService(out _, out _);

        // Act
        var premier = svc.GenererMotDePasse();
        var second = svc.GenererMotDePasse();

        // Assert
        Assert.True(premier.Length >= AbonnementActivationService.LongueurMinMotDePasse);
        Assert.NotEqual(premier, second);
    }

    [Fact]
    public async Task EnregistrerAsync_WhenActivatedWithoutCompany_ShouldCreateCompanyAccountAndSendCredentials()
    {
        // Arrange
        var svc = CreateService(out var db, out var emails);
        var demande = AjouterDemande(db);
        demande.Statut = "Essai";

        // Act
        var resultat = await svc.EnregistrerAsync(demande, Acces, envoyerEmail: true);

        // Assert
        var entreprise = await db.Companies.SingleAsync(c => c.Name == "Société Exemple");
        Assert.Equal("Pro", entreprise.Plan);
        Assert.Equal(entreprise.Id, demande.CompanyId);

        var compte = await db.Utilisateurs.IgnoreQueryFilters().SingleAsync(u => u.Login == "karim.ben.ali");
        Assert.Equal(entreprise.Id, compte.CompanyId);
        Assert.Equal(RoleNameMapper.Admin, compte.Role);
        Assert.Equal("karim@exemple.tn", compte.Email);
        Assert.True(compte.Actif);
        Assert.NotEqual("Motdepasse9", compte.PasswordHash);

        // La nouvelle entreprise reçoit sa propre fiche (en-tête des documents), pré-remplie.
        var fiche = await db.Entreprises.IgnoreQueryFilters().SingleAsync(e => e.CompanyId == entreprise.Id);
        Assert.Equal("Société Exemple", fiche.NomEntreprise);
        Assert.Equal("karim@exemple.tn", fiche.Email);
        Assert.Equal(Entreprise.CodePourCompany(entreprise.Id), fiche.CodeEntreprise);

        Assert.True(resultat.EntrepriseCreee);
        Assert.Equal("karim.ben.ali", resultat.LoginCree);
        var email = Assert.Single(emails.Sent);
        Assert.Equal("karim@exemple.tn", email.To);
        Assert.Contains("karim.ben.ali", email.Html);
        Assert.Contains("Motdepasse9", email.Html);
        Assert.Contains("/compte/connexion", email.Html);
    }

    [Fact]
    public async Task EnregistrerAsync_WhenAccountCreated_ShouldAllowClientToLogIn()
    {
        // Arrange
        var svc = CreateService(out var db, out _);
        var demande = AjouterDemande(db);
        demande.Statut = "Active";

        // Act
        await svc.EnregistrerAsync(demande, Acces, envoyerEmail: false);

        // Assert
        var utilisateurs = new UtilisateurService(db, new StubTenantService());
        Assert.NotNull(await utilisateurs.AuthentifierAsync("karim.ben.ali", "Motdepasse9"));
    }

    [Fact]
    public async Task EnregistrerAsync_WhenCompanyAlreadyHasAccount_ShouldNotCreateAnotherAndSendUpdateEmailWithoutPassword()
    {
        // Arrange
        var svc = CreateService(out var db, out var emails);
        var entreprise = new Company { Name = "Société Exemple", Plan = "Pro" };
        db.Companies.Add(entreprise);
        db.SaveChanges();
        db.Utilisateurs.Add(new Utilisateur { Login = "existant", Prenom = "A", Nom = "B", PasswordHash = "x", CompanyId = entreprise.Id, Role = "Admin" });
        db.SaveChanges();
        var demande = AjouterDemande(db, statut: "Essai", companyId: entreprise.Id);
        demande.Statut = "Active";

        // Act
        var resultat = await svc.EnregistrerAsync(demande, acces: null, envoyerEmail: true);

        // Assert
        Assert.False(resultat.EntrepriseCreee);
        Assert.Null(resultat.LoginCree);
        Assert.Equal(1, await db.Utilisateurs.IgnoreQueryFilters().CountAsync(u => u.CompanyId == entreprise.Id));
        var email = Assert.Single(emails.Sent);
        Assert.Contains("mis à jour", email.Subject);
        Assert.DoesNotContain("Mot de passe", email.Html);
    }

    [Theory]
    [InlineData("EnAttente")]
    [InlineData("Contactee")]
    [InlineData("Refusee")]
    public async Task EnregistrerAsync_WhenNotActivated_ShouldCreateNothingAndSendNoEmail(string statut)
    {
        // Arrange
        var svc = CreateService(out var db, out var emails);
        var demande = AjouterDemande(db);
        demande.Statut = statut;
        demande.NotesAdmin = "Rappeler lundi";

        // Act
        var resultat = await svc.EnregistrerAsync(demande, acces: null, envoyerEmail: true);

        // Assert
        Assert.Empty(db.Companies.Where(c => c.Name == "Société Exemple"));
        Assert.Null(demande.CompanyId);
        Assert.False(resultat.EmailDemande);
        Assert.Empty(emails.Sent);
        Assert.Equal("Rappeler lundi", (await db.Abonnements.AsNoTracking().SingleAsync()).NotesAdmin);
    }

    [Fact]
    public async Task EnregistrerAsync_WhenEmailUnchecked_ShouldActivateWithoutSendingEmail()
    {
        // Arrange
        var svc = CreateService(out var db, out var emails);
        var demande = AjouterDemande(db);
        demande.Statut = "Essai";

        // Act
        await svc.EnregistrerAsync(demande, Acces, envoyerEmail: false);

        // Assert
        Assert.NotNull(demande.CompanyId);
        Assert.Empty(emails.Sent);
    }

    [Fact]
    public async Task EnregistrerAsync_WhenLoginAlreadyUsed_ShouldThrowAndCreateNothing()
    {
        // Arrange
        var svc = CreateService(out var db, out var emails);
        db.Utilisateurs.Add(new Utilisateur { Login = "karim.ben.ali", Prenom = "A", Nom = "B", PasswordHash = "x" });
        db.SaveChanges();
        var demande = AjouterDemande(db);
        demande.Statut = "Essai";

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => svc.EnregistrerAsync(demande, Acces, envoyerEmail: true));

        // Assert
        Assert.Contains("déjà utilisé", ex.Message);
        Assert.Empty(db.Companies.Where(c => c.Name == "Société Exemple"));
        Assert.Empty(emails.Sent);
    }

    [Fact]
    public async Task EnregistrerAsync_WhenPasswordTooShort_ShouldThrow()
    {
        // Arrange
        var svc = CreateService(out var db, out _);
        var demande = AjouterDemande(db);
        demande.Statut = "Essai";

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.EnregistrerAsync(demande, new AccesCompte("karim.ben.ali", "court"), envoyerEmail: true));

        // Assert
        Assert.Contains("au moins", ex.Message);
    }

    [Fact]
    public async Task EnregistrerAsync_WhenCompanyNameAlreadyExists_ShouldAskToLinkItInsteadOfDuplicating()
    {
        // Arrange
        var svc = CreateService(out var db, out _);
        db.Companies.Add(new Company { Name = "Société Exemple", Plan = "Standard" });
        db.SaveChanges();
        var demande = AjouterDemande(db);
        demande.Statut = "Essai";

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => svc.EnregistrerAsync(demande, Acces, envoyerEmail: true));

        // Assert
        Assert.Contains("existe déjà", ex.Message);
        Assert.Equal(1, await db.Companies.CountAsync(c => c.Name == "Société Exemple"));
    }

    // ── Licence Desktop : rien à créer sur le web ─────────────────────────

    [Fact]
    public async Task EnregistrerAsync_WhenDesktopLicenceActivated_ShouldCreateNoCompanyNoAccountAndSendNoEmail()
    {
        // Arrange — le logiciel tourne chez le client : pas d'entreprise ni de compte sur le web.
        var svc = CreateService(out var db, out var emails);
        var demande = AjouterDemande(db);
        demande.Plan = TarifsOptions.PlanDesktopSerenite;
        demande.Statut = "Active";
        demande.NotesAdmin = "Licence livrée le 08/10";

        // Act
        Assert.False(await svc.CompteRequisAsync(demande));
        var resultat = await svc.EnregistrerAsync(demande, acces: null, envoyerEmail: true);

        // Assert
        Assert.False(resultat.EntrepriseCreee);
        Assert.Null(resultat.LoginCree);
        Assert.False(resultat.EmailDemande);
        Assert.Null(demande.CompanyId);
        Assert.Empty(db.Companies.Where(c => c.Name == "Société Exemple"));
        Assert.Empty(emails.Sent);
        Assert.Equal("Active", (await db.Abonnements.AsNoTracking().SingleAsync()).Statut);
    }
}
