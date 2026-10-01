using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Moq;
using Web_GestCom.Data.Models;
using Web_GestCom.Pages.Compte;
using Web_GestCom.Services;
using Xunit;

namespace Web_GestCom.Tests.Pages;

public class DemandeAbonnementModelTests
{
    private static DemandeAbonnementModel CreateModel(bool antiforgeryValid, out Mock<IAbonnementService> abonnementService)
    {
        abonnementService = new Mock<IAbonnementService>();
        abonnementService.Setup(s => s.CreateDemandeAsync(It.IsAny<Abonnement>()))
            .ReturnsAsync((Abonnement a) => a);

        var antiforgery = new Mock<IAntiforgery>();
        antiforgery.Setup(a => a.IsRequestValidAsync(It.IsAny<HttpContext>())).ReturnsAsync(antiforgeryValid);

        return new DemandeAbonnementModel(abonnementService.Object, antiforgery.Object, Options.Create(new TarifsOptions()))
        {
            PageContext = new PageContext { HttpContext = new DefaultHttpContext() },
            Input = new DemandeAbonnementModel.InputModel
            {
                NomEntreprise = "Entreprise Test",
                NomContact = "Contact Test",
                EmailContact = "contact@test.com",
                Plan = "Pro"
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
    public async Task OnPostAsync_WhenPromoCodeInvalid_ShouldRedisplayFormWithFieldErrorAndNotSave()
    {
        // Arrange
        var model = CreateModel(antiforgeryValid: true, out var abonnementService);
        abonnementService.Setup(s => s.GetOffrePromoAsync("FAUX")).ReturnsAsync((OffrePromo?)null);
        model.Input.CodePromo = "FAUX";

        // Act
        var result = await model.OnPostAsync();

        // Assert
        Assert.IsType<PageResult>(result);
        Assert.False(model.DemandeEnvoyee);
        Assert.Contains(model.ModelState["Input.CodePromo"]!.Errors,
            e => e.ErrorMessage == AbonnementService.CodePromoInvalideMessage);
        abonnementService.Verify(s => s.CreateDemandeAsync(It.IsAny<Abonnement>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_ShouldPassCycleAndPromoCodeToService()
    {
        // Arrange
        var model = CreateModel(antiforgeryValid: true, out var abonnementService);
        abonnementService.Setup(s => s.GetOffrePromoAsync("FONDATEUR2026"))
            .ReturnsAsync(new OffrePromo("FONDATEUR2026", "Client Fondateur", 35, 10, 10));
        model.Input.CycleFacturation = "Mensuel";
        model.Input.CodePromo = "FONDATEUR2026";

        // Act
        await model.OnPostAsync();

        // Assert
        abonnementService.Verify(s => s.CreateDemandeAsync(It.Is<Abonnement>(a =>
            a.CycleFacturation == "Mensuel" && a.CodePromo == "FONDATEUR2026")), Times.Once);
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
}
