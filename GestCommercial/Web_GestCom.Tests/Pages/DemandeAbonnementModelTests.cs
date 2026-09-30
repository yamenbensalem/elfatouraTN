using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        return new DemandeAbonnementModel(abonnementService.Object, antiforgery.Object)
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
}
