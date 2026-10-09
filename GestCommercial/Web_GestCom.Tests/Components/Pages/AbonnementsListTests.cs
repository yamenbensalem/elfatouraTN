using Bunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Web_GestCom.Components.Pages.Admin;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;
using Xunit;

namespace Web_GestCom.Tests.Components.Pages;

public sealed class AbonnementsListTests : TestContext
{
    private readonly Mock<IAbonnementService> _abonnements = new();
    private readonly Mock<IAbonnementActivationService> _activation = new();
    private readonly Mock<ICompanyService> _companies = new();

    public AbonnementsListTests()
    {
        Services.AddScoped(_ => _abonnements.Object);
        Services.AddScoped(_ => _activation.Object);
        Services.AddScoped(_ => _companies.Object);
        Services.AddSingleton(Options.Create(new TarifsOptions()));
        _companies.Setup(s => s.GetAllAsync()).ReturnsAsync([]);
        _activation.Setup(s => s.SuggererLoginAsync(It.IsAny<string>())).ReturnsAsync("login");
        _activation.Setup(s => s.GenererMotDePasse()).Returns("MotDePasse12");
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("superadmin");
        auth.SetRoles("SuperAdmin");
    }

    private static Abonnement Client(int id, string nom, string statut, int? echeanceDansJours, string plan = "Pro", string cycle = "Annuel") => new()
    {
        Id = id, NomEntreprise = nom, NomContact = "Contact", EmailContact = $"contact{id}@exemple.tn",
        Plan = plan, CycleFacturation = cycle, Statut = statut, PrixApplique = 590,
        DateDebut = echeanceDansJours.HasValue ? DateTime.Today.AddMonths(-11) : null,
        DateEcheance = echeanceDansJours.HasValue ? DateTime.Today.AddDays(echeanceDansJours.Value) : null
    };

    private IRenderedComponent<AbonnementsList> Render(params Abonnement[] clients)
    {
        _abonnements.Setup(s => s.GetAllAsync()).ReturnsAsync([.. clients]);
        var cut = RenderComponent<AbonnementsList>();
        cut.WaitForElement("table");
        return cut;
    }

    private static List<string> Lignes(IRenderedComponent<AbonnementsList> cut)
        => cut.FindAll("tbody tr").Select(tr => tr.TextContent).ToList();

    [Fact]
    public void List_ShouldShowStartDateDueDateAndFollowUpStateForEachClient()
    {
        // Arrange
        var cut = Render(Client(1, "Société En Cours", "Active", echeanceDansJours: 200));

        // Act
        var ligne = Lignes(cut).Single();

        // Assert
        Assert.Contains(DateTime.Today.AddMonths(-11).ToString("dd/MM/yyyy"), ligne);
        Assert.Contains(DateTime.Today.AddDays(200).ToString("dd/MM/yyyy"), ligne);
        Assert.Contains("En cours : 200 jours restants", ligne);
        Assert.DoesNotContain("Relancer", ligne);
    }

    [Fact]
    public void List_ShouldPutClientsNeedingActionFirst()
    {
        // Arrange — volontairement dans le désordre.
        var cut = Render(
            Client(1, "En cours", "Active", 200),
            Client(2, "Demande nouvelle", "EnAttente", null),
            Client(3, "A relancer", "Active", 10),
            Client(4, "Sans dates", "Active", null),
            Client(5, "Expire", "Active", -4));

        // Act
        var ordre = cut.FindAll("tbody tr .fw-semibold").Select(e => e.TextContent).ToList();

        // Assert
        Assert.Equal(["Expire", "A relancer", "Sans dates", "Demande nouvelle", "En cours"], ordre);
        Assert.Contains("Échéance dépassée : suspension dans 10 jours", Lignes(cut)[0]);
        Assert.Contains("À relancer : 10 jours restants", Lignes(cut)[1]);
        Assert.Contains("Dates à renseigner", Lignes(cut)[2]);
    }

    [Fact]
    public void Filter_ShouldShowCountsAndNarrowTheList()
    {
        // Arrange
        var cut = Render(Client(1, "En cours", "Active", 200), Client(3, "A relancer", "Active", 10), Client(5, "Expire", "Active", -4));
        var filtre = cut.FindAll("[aria-label='Filtrer les clients'] button").Single(b => b.TextContent.Contains("À relancer"));
        Assert.Contains("1", filtre.TextContent);

        // Act
        filtre.Click();

        // Assert
        Assert.Equal(["A relancer"], cut.FindAll("tbody tr .fw-semibold").Select(e => e.TextContent));
    }

    [Fact]
    public void Relancer_WhenEmailSent_ShouldCallServiceAndConfirm()
    {
        // Arrange
        _abonnements.Setup(s => s.RelancerAsync(3)).ReturnsAsync(true);
        var cut = Render(Client(3, "A relancer", "Active", 10));

        // Act
        cut.FindAll("button").Single(b => b.TextContent.Contains("Relancer")).Click();

        // Assert
        cut.WaitForAssertion(() => Assert.Contains("Email de relance envoyé à contact3@exemple.tn", cut.Markup));
        _abonnements.Verify(s => s.RelancerAsync(3), Times.Once);
    }

    [Fact]
    public void Relancer_WhenEmailNotSent_ShouldSaySoInsteadOfConfirming()
    {
        // Arrange
        _abonnements.Setup(s => s.RelancerAsync(3)).ReturnsAsync(false);
        var cut = Render(Client(3, "A relancer", "Active", 10));

        // Act
        cut.FindAll("button").Single(b => b.TextContent.Contains("Relancer")).Click();

        // Assert
        cut.WaitForAssertion(() => Assert.Contains("n'a pas pu être envoyé", cut.Markup));
        Assert.DoesNotContain("Email de relance envoyé", cut.Markup);
    }

    [Fact]
    public void List_WhenAlreadyReminded_ShouldShowWhenAndOfferToRemindAgain()
    {
        // Arrange
        var client = Client(3, "A relancer", "Active", 10);
        client.DateDerniereRelance = DateTime.UtcNow.AddDays(-2);

        // Act
        var cut = Render(client);

        // Assert
        var ligne = Lignes(cut).Single();
        Assert.Contains("Relancé le", ligne);
        Assert.Contains("Relancer encore", ligne);
    }

    [Fact]
    public void Traiter_WhenActivatingARequestWithoutDates_ShouldProposeThemFromTheOffer()
    {
        // Arrange
        var cut = Render(Client(2, "Demande nouvelle", "EnAttente", null));
        cut.FindAll("button").Single(b => b.TextContent.Contains("Traiter")).Click();

        // Act
        cut.Find("select").Change("Active");

        // Assert — abonnement annuel : un an à partir d'aujourd'hui.
        var dates = cut.FindAll("input[type=date]").Select(i => i.GetAttribute("value")).ToList();
        Assert.Equal([DateTime.Today.ToString("yyyy-MM-dd"), DateTime.Today.AddYears(1).ToString("yyyy-MM-dd")], dates);
        Assert.Contains("Dates proposées d'après l'offre", cut.Markup);
    }

    [Fact]
    public void List_WhenGracePeriodIsOver_ShouldShowAccessSuspendedFirstAndStillOfferToRemind()
    {
        // Arrange — 14 jours de grâce écoulés depuis 6 jours.
        var cut = Render(Client(1, "En retard", "Active", -4), Client(2, "Suspendu", "Active", -20));

        // Act
        var lignes = Lignes(cut);

        // Assert
        Assert.Contains("Suspendu", lignes[0]);
        Assert.Contains("Accès suspendu depuis 6 jours", lignes[0]);
        Assert.Contains("Relancer", lignes[0]);
    }

    [Fact]
    public void List_WhenTrialEndedYesterday_ShouldShowAccessSuspended()
    {
        // Arrange — pas de délai de grâce pour un essai gratuit.
        var cut = Render(Client(1, "Essai fini", "Essai", -1, plan: "Standard"));

        // Act + Assert
        Assert.Contains("Accès suspendu depuis 1 jour", Lignes(cut).Single());
    }

    [Fact]
    public void List_WhenDesktopMaintenanceEnded_ShouldNotTalkAboutSuspension()
    {
        // Arrange
        var cut = Render(Client(1, "Licence", "Active", -40, plan: TarifsOptions.PlanDesktopSerenite));

        // Act
        var ligne = Lignes(cut).Single();

        // Assert
        Assert.Contains("Maintenance terminée depuis 40 jours", ligne);
        Assert.DoesNotContain("suspen", ligne, StringComparison.OrdinalIgnoreCase);
    }
}
