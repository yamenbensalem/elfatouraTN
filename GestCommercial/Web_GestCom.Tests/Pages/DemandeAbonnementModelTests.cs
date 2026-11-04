using System.Globalization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Web_GestCom.Data.Models;
using Web_GestCom.Pages.Compte;
using Web_GestCom.Services;
using Xunit;

namespace Web_GestCom.Tests.Pages;

public class DemandeAbonnementModelTests
{
    // Une instance de classe par test (xUnit) : un fournisseur de clés éphémère par test, partagé entre
    // le modèle de page et les jetons que les tests fabriquent à la main.
    private readonly EphemeralDataProtectionProvider _protection = new();

    private string Token(TimeSpan age)
        => _protection.CreateProtector(DemandeAbonnementModel.FormTokenPurpose)
            .Protect((DateTimeOffset.UtcNow - age).UtcTicks.ToString(CultureInfo.InvariantCulture));

    private DemandeAbonnementModel CreateModel(
        bool antiforgeryValid,
        out Mock<IAbonnementService> abonnementService,
        IPublicFormRateLimiter? rateLimiter = null,
        string? formToken = null)
    {
        abonnementService = new Mock<IAbonnementService>();
        abonnementService.Setup(s => s.CreateDemandeAsync(It.IsAny<Abonnement>()))
            .ReturnsAsync((Abonnement a) => a);

        var antiforgery = new Mock<IAntiforgery>();
        antiforgery.Setup(a => a.IsRequestValidAsync(It.IsAny<HttpContext>())).ReturnsAsync(antiforgeryValid);

        return new DemandeAbonnementModel(
            abonnementService.Object,
            antiforgery.Object,
            Options.Create(new TarifsOptions()),
            _protection,
            rateLimiter ?? new InMemoryPublicFormRateLimiter(),
            NullLogger<DemandeAbonnementModel>.Instance)
        {
            PageContext = new PageContext { HttpContext = new DefaultHttpContext() },
            Input = new DemandeAbonnementModel.InputModel
            {
                NomEntreprise = "Entreprise Test",
                NomContact = "Contact Test",
                EmailContact = "contact@test.com",
                Plan = "Pro",
                FormToken = formToken ?? Token(TimeSpan.FromSeconds(30))
            }
        };
    }

    // Régression prod 2026-09-27 : formulaire chargé déconnecté, connexion dans un autre onglet,
    // puis envoi → le jeton antiforgery ne correspond plus à l'utilisateur → 400 vide.
    [Fact]
    public async Task OnPostAsync_WhenAntiforgeryTokenInvalid_ShouldRedisplayFormWithMessageAndNotSave()
    {
        // Arrange
        var model = CreateModel(antiforgeryValid: false, out var abonnementService);

        // Act
        var result = await model.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.False(model.DemandeEnvoyee);
        Assert.Contains(model.ModelState[string.Empty]!.Errors,
            e => e.ErrorMessage == DemandeAbonnementModel.SessionChangeeMessage);
        Assert.Equal("Entreprise Test", model.Input.NomEntreprise);
        abonnementService.Verify(s => s.CreateDemandeAsync(It.IsAny<Abonnement>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenAntiforgeryTokenValid_ShouldSaveDemande()
    {
        // Arrange
        var model = CreateModel(antiforgeryValid: true, out var abonnementService);

        // Act
        var result = await model.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.True(model.DemandeEnvoyee);
        abonnementService.Verify(s => s.CreateDemandeAsync(It.Is<Abonnement>(a =>
            a.NomEntreprise == "Entreprise Test" && a.EmailContact == "contact@test.com" && a.Plan == "Pro")), Times.Once);
    }

    [Fact]
    public async Task OnPostAsync_ShouldPassCycleToService_AndNeverAPromoCode()
    {
        // Arrange — le champ code promo a été retiré du formulaire avec l'offre Fondateurs.
        var model = CreateModel(antiforgeryValid: true, out var abonnementService);
        model.Input.CycleFacturation = "Mensuel";

        // Act
        await model.OnPostAsync();

        // Assert
        abonnementService.Verify(s => s.CreateDemandeAsync(It.Is<Abonnement>(a =>
            a.CycleFacturation == "Mensuel" && a.CodePromo == null)), Times.Once);
    }

    [Fact]
    public void OnGet_ShouldPrefillPlanAndCycleFromPricingPage()
    {
        // Arrange
        var model = CreateModel(antiforgeryValid: true, out _);

        // Act
        model.OnGet("Standard", "Mensuel");

        // Assert
        Assert.Equal("Standard", model.Input.Plan);
        Assert.Equal("Mensuel", model.Input.CycleFacturation);
    }

    [Theory]
    [InlineData("DesktopEssentiel")]
    [InlineData("DesktopSerenite")]
    [InlineData("DesktopEquipe")]
    public async Task DesktopFormula_ShouldBePreselectedFromThePricingPageAndSaved(string plan)
    {
        // Arrange
        var model = CreateModel(antiforgeryValid: true, out var abonnementService);

        // Act
        model.OnGet(plan, null);
        model.Input.FormToken = Token(TimeSpan.FromSeconds(30)); // OnGet vient d'émettre un jeton tout neuf
        await model.OnPostAsync();

        // Assert
        Assert.Equal(plan, model.Input.Plan);
        Assert.True(model.DemandeEnvoyee);
        abonnementService.Verify(s => s.CreateDemandeAsync(It.Is<Abonnement>(a => a.Plan == plan)), Times.Once);
    }

    [Theory]
    [InlineData("Desktop")]
    [InlineData("Gratuit")]
    public async Task OnPostAsync_WhenUnknownPlan_ShouldRejectWithoutSaving(string plan)
    {
        // Arrange — "Desktop" seul n'est pas une formule : il faut l'une des trois.
        var model = CreateModel(antiforgeryValid: true, out var abonnementService);
        model.Input.Plan = plan;

        // Act
        await model.OnPostAsync();

        // Assert
        Assert.False(model.DemandeEnvoyee);
        Assert.True(model.ModelState.ContainsKey("Input.Plan"));
        abonnementService.Verify(s => s.CreateDemandeAsync(It.IsAny<Abonnement>()), Times.Never);
    }

    // ── Protection anti-robot (envoi en masse de ~800 fausses demandes, 2026-11-04) ──

    [Fact]
    public void OnGet_ShouldIssueAFormToken()
    {
        // Arrange
        var model = CreateModel(antiforgeryValid: true, out _, formToken: "");

        // Act
        model.OnGet(null, null);

        // Assert
        Assert.False(string.IsNullOrEmpty(model.Input.FormToken));
    }

    [Fact]
    public async Task OnPostAsync_WhenHoneypotFilled_ShouldPretendSuccessWithoutSaving()
    {
        // Arrange — un robot remplit tous les champs, y compris celui qu'un humain ne voit pas.
        var model = CreateModel(antiforgeryValid: true, out var abonnementService);
        model.Input.Commentaire = "Super offre !!!";

        // Act
        await model.OnPostAsync();

        // Assert
        Assert.True(model.DemandeEnvoyee);
        abonnementService.Verify(s => s.CreateDemandeAsync(It.IsAny<Abonnement>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenFormTokenMissing_ShouldRejectAndIssueANewToken()
    {
        // Arrange
        var model = CreateModel(antiforgeryValid: true, out var abonnementService, formToken: "");

        // Act
        await model.OnPostAsync();

        // Assert
        Assert.False(model.DemandeEnvoyee);
        Assert.Contains(model.ModelState[string.Empty]!.Errors, e => e.ErrorMessage == DemandeAbonnementModel.FormulaireExpireMessage);
        Assert.False(string.IsNullOrEmpty(model.Input.FormToken));
        abonnementService.Verify(s => s.CreateDemandeAsync(It.IsAny<Abonnement>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenFormTokenForgedOrFromAnotherKey_ShouldReject()
    {
        // Arrange — jeton fabriqué avec une autre clé : indéchiffrable pour ce serveur.
        var foreign = new EphemeralDataProtectionProvider().CreateProtector(DemandeAbonnementModel.FormTokenPurpose)
            .Protect(DateTimeOffset.UtcNow.AddMinutes(-5).UtcTicks.ToString(CultureInfo.InvariantCulture));
        var model = CreateModel(antiforgeryValid: true, out var abonnementService, formToken: foreign);

        // Act
        await model.OnPostAsync();

        // Assert
        Assert.False(model.DemandeEnvoyee);
        abonnementService.Verify(s => s.CreateDemandeAsync(It.IsAny<Abonnement>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenSubmittedTooFast_ShouldRejectWithRetryableMessage()
    {
        // Arrange
        var model = CreateModel(antiforgeryValid: true, out var abonnementService, formToken: Token(TimeSpan.FromSeconds(1)));

        // Act
        await model.OnPostAsync();

        // Assert
        Assert.False(model.DemandeEnvoyee);
        Assert.Contains(model.ModelState[string.Empty]!.Errors, e => e.ErrorMessage == DemandeAbonnementModel.EnvoiTropRapideMessage);
        abonnementService.Verify(s => s.CreateDemandeAsync(It.IsAny<Abonnement>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenFormTokenOlderThan24Hours_ShouldReject()
    {
        // Arrange
        var model = CreateModel(antiforgeryValid: true, out var abonnementService, formToken: Token(TimeSpan.FromHours(25)));

        // Act
        await model.OnPostAsync();

        // Assert
        Assert.False(model.DemandeEnvoyee);
        Assert.Contains(model.ModelState[string.Empty]!.Errors, e => e.ErrorMessage == DemandeAbonnementModel.FormulaireExpireMessage);
        abonnementService.Verify(s => s.CreateDemandeAsync(It.IsAny<Abonnement>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenIpExceedsHourlyCap_ShouldRejectFurtherRequests()
    {
        // Arrange
        var limiter = new InMemoryPublicFormRateLimiter();
        for (var i = 0; i < DemandeAbonnementModel.MaxEnvoisParIpParHeure; i++)
        {
            var accepted = CreateModel(antiforgeryValid: true, out _, limiter);
            await accepted.OnPostAsync();
            Assert.True(accepted.DemandeEnvoyee);
        }
        var model = CreateModel(antiforgeryValid: true, out var abonnementService, limiter);

        // Act
        await model.OnPostAsync();

        // Assert
        Assert.False(model.DemandeEnvoyee);
        Assert.Contains(model.ModelState[string.Empty]!.Errors, e => e.ErrorMessage == DemandeAbonnementModel.TropDeDemandesMessage);
        abonnementService.Verify(s => s.CreateDemandeAsync(It.IsAny<Abonnement>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenForwardedForFirstHopIsSpoofed_ShouldStillApplyTheCap()
    {
        // Arrange — le client invente un premier saut X-Forwarded-For différent à chaque requête ; le
        // dernier saut (ajouté par nginx-proxy) reste le même et c'est lui qui compte.
        var limiter = new InMemoryPublicFormRateLimiter();
        for (var i = 0; i < DemandeAbonnementModel.MaxEnvoisParIpParHeure; i++)
        {
            var m = CreateModel(antiforgeryValid: true, out _, limiter);
            m.HttpContext.Request.Headers["X-Forwarded-For"] = $"10.0.0.{i}, 203.0.113.7";
            await m.OnPostAsync();
        }
        var model = CreateModel(antiforgeryValid: true, out var abonnementService, limiter);
        model.HttpContext.Request.Headers["X-Forwarded-For"] = "10.9.9.9, 203.0.113.7";

        // Act
        await model.OnPostAsync();

        // Assert
        Assert.False(model.DemandeEnvoyee);
        abonnementService.Verify(s => s.CreateDemandeAsync(It.IsAny<Abonnement>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WhenSameEmailAndPlanAlreadyRequestedRecently_ShouldRejectWithoutSaving()
    {
        // Arrange
        var model = CreateModel(antiforgeryValid: true, out var abonnementService);
        abonnementService.Setup(s => s.ExisteDemandeRecenteAsync("contact@test.com", "Pro")).ReturnsAsync(true);

        // Act
        await model.OnPostAsync();

        // Assert
        Assert.False(model.DemandeEnvoyee);
        Assert.Contains(model.ModelState[string.Empty]!.Errors, e => e.ErrorMessage == DemandeAbonnementModel.DemandeDejaEnCoursMessage);
        abonnementService.Verify(s => s.CreateDemandeAsync(It.IsAny<Abonnement>()), Times.Never);
    }
}
