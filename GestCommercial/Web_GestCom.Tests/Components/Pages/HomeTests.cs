using Bunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Web_GestCom.Components.Pages;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;
using Xunit;

namespace Web_GestCom.Tests.Components.Pages;

public sealed class HomeTests : TestContext
{
    private readonly Mock<IClientService>        _clients      = new();
    private readonly Mock<IProduitService>       _produits     = new();
    private readonly Mock<IFournisseurService>   _fournisseurs = new();
    private readonly Mock<IFactureClientService> _factures     = new();
    private readonly Mock<ICompanyService>       _companies    = new();
    private readonly Mock<ICurrentUserService>   _currentUser  = new();
    private readonly Mock<IPermissionService>    _permissions  = new();
    private readonly Mock<IAbonnementService>    _abonnements  = new();
    private readonly Mock<IEntrepriseService>    _entreprise   = new();

    public HomeTests()
    {
        Services.AddScoped(_ => _clients.Object);
        Services.AddScoped(_ => _produits.Object);
        Services.AddScoped(_ => _fournisseurs.Object);
        Services.AddScoped(_ => _factures.Object);
        Services.AddScoped(_ => _companies.Object);
        Services.AddScoped(_ => _currentUser.Object);
        Services.AddSingleton(_permissions.Object);
        Services.AddScoped(_ => _abonnements.Object);
        Services.AddScoped(_ => _entreprise.Object);
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new TarifsOptions()));

        // Default: empty collections so each test only sets up what it needs
        _clients.Setup(s => s.GetAllAsync(null)).ReturnsAsync([]);
        _produits.Setup(s => s.GetAllAsync(null, null)).ReturnsAsync([]);
        _produits.Setup(s => s.GetStockAlerteAsync()).ReturnsAsync([]);
        _fournisseurs.Setup(s => s.GetAllAsync(null)).ReturnsAsync([]);
        _factures.Setup(s => s.GetAllAsync(false, null)).ReturnsAsync([]);
        _factures.Setup(s => s.GetAllAsync(true, null)).ReturnsAsync([]);
        _companies.Setup(s => s.GetAllAsync()).ReturnsAsync([]);
        _currentUser.Setup(s => s.Login).Returns("testuser");
        _currentUser.Setup(s => s.IsAuthenticated).Returns(true);
        _currentUser.Setup(s => s.IsSuperAdmin).Returns(false);
    }

    private void AuthorizeAdmin()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("testuser");
        auth.SetRoles("Admin");
    }

    // ── Anonymous landing page ───────────────────────────────────────────

    [Fact]
    public void AnonymousVisitor_SeesPublicLandingPage_NotTheDashboard()
    {
        _currentUser.Setup(s => s.IsAuthenticated).Returns(false);
        var auth = this.AddTestAuthorization();
        auth.SetNotAuthorized();
        JSInterop.SetupVoid("gcInitLandingReveal");

        var cut = RenderComponent<Home>();

        Assert.Contains("Se connecter", cut.Markup);
        Assert.Contains("Des tarifs simples", cut.Markup);
        Assert.DoesNotContain("Bienvenue,", cut.Markup);
        _clients.Verify(s => s.GetAllAsync(null), Times.Never);
        _companies.Verify(s => s.GetAllAsync(), Times.Never);
    }

    // ── Greeting ──────────────────────────────────────────────────────────

    [Fact]
    public void RendersLoginNameInGreeting()
    {
        _currentUser.Setup(s => s.Login).Returns("yamen");
        AuthorizeAdmin();

        var cut = RenderComponent<Home>();
        cut.WaitForState(
            () => !cut.Markup.Contains("spinner-border"),
            TimeSpan.FromSeconds(5));

        Assert.Contains("yamen", cut.Markup);
    }

    // ── KPI Cards ─────────────────────────────────────────────────────────

    [Fact]
    public void KpiCards_ShowClientCount()
    {
        _clients.Setup(s => s.GetAllAsync(null)).ReturnsAsync(
        [
            new Client { CodeClient = "CL00001", NomClient = "Alpha SARL", CodeDevise = 1 },
            new Client { CodeClient = "CL00002", NomClient = "Beta Corp",  CodeDevise = 1 },
            new Client { CodeClient = "CL00003", NomClient = "Gamma Ltd",  CodeDevise = 1 }
        ]);
        AuthorizeAdmin();

        var cut = RenderComponent<Home>();
        cut.WaitForState(
            () => cut.Markup.Contains("Total Clients"),
            TimeSpan.FromSeconds(5));

        Assert.Contains("3", cut.Markup);
    }

    [Fact]
    public void KpiCards_ShowOpenInvoiceCount()
    {
        _factures.Setup(s => s.GetAllAsync(false, null)).ReturnsAsync(
        [
            new FactureClient { NumeroFactureClient = "FC001", EtatReglement = "Non Réglé", DateFactureClient = DateTime.Today, MontantTTC = 100 },
            new FactureClient { NumeroFactureClient = "FC002", EtatReglement = "Réglé",     DateFactureClient = DateTime.Today, MontantTTC = 200 },
            new FactureClient { NumeroFactureClient = "FC003", EtatReglement = "Non Réglé", DateFactureClient = DateTime.Today, MontantTTC = 150 }
        ]);
        AuthorizeAdmin();

        var cut = RenderComponent<Home>();
        cut.WaitForState(
            () => cut.Markup.Contains("Factures en attente"),
            TimeSpan.FromSeconds(5));

        Assert.Contains("2", cut.Markup);  // 2 factures non-réglées
    }

    [Fact]
    public void KpiCards_ShowUnpaidAmount_NetOfReglements()
    {
        _factures.Setup(s => s.GetAllAsync(false, null)).ReturnsAsync(
        [
            new FactureClient
            {
                NumeroFactureClient = "FC001",
                EtatReglement       = "Partiellement Réglé",
                DateFactureClient   = DateTime.Today,
                MontantTTC          = 200,
                Timbre              = 0.6,
                Reglements          = [new ReglementFactureClient { Montant = 50, DateReglement = DateTime.Today, CodeModePayement = 1 }]
            }
        ]);
        AuthorizeAdmin();

        var cut = RenderComponent<Home>();
        cut.WaitForState(
            () => cut.Markup.Contains("Montant Impayé"),
            TimeSpan.FromSeconds(5));

        // 200 + 0.6 timbre - 50 déjà réglé = 150.6 (formaté selon la culture courante : "." ou ",")
        var expected = (200.0 + 0.6 - 50).ToString("0.###");
        Assert.Contains(expected, cut.Markup);
    }

    [Fact]
    public void KpiCards_ShowCaDuMois_ExcludesInvoicesFromOtherMonths()
    {
        // Le graphique mensuel affiche légitimement les 6 derniers mois (donc "lastMonth" y apparaît
        // aussi) — ce test vérifie seulement la carte KPI "CA du Mois", pas la page entière.
        var thisMonth = DateTime.Today;
        var lastMonth = thisMonth.AddMonths(-2);
        _factures.Setup(s => s.GetAllAsync(false, null)).ReturnsAsync(
        [
            new FactureClient { NumeroFactureClient = "FC001", EtatReglement = "Réglé", DateFactureClient = thisMonth, MontantTTC = 300 },
            new FactureClient { NumeroFactureClient = "FC002", EtatReglement = "Réglé", DateFactureClient = lastMonth, MontantTTC = 999 }
        ]);
        AuthorizeAdmin();

        var cut = RenderComponent<Home>();
        cut.WaitForState(
            () => cut.Markup.Contains("CA du Mois"),
            TimeSpan.FromSeconds(5));

        var kpiCard = cut.FindAll(".kpi-label").Single(e => e.TextContent.Contains("CA du Mois")).ParentElement!.ParentElement!;
        Assert.Contains("300", kpiCard.TextContent);
        Assert.DoesNotContain("999", kpiCard.TextContent);
    }

    [Fact]
    public void KpiCards_CaDuMois_SubtractsAvoirsFromSameMonth()
    {
        var thisMonth = DateTime.Today;
        _factures.Setup(s => s.GetAllAsync(false, null)).ReturnsAsync(
        [
            new FactureClient { NumeroFactureClient = "FC001", EtatReglement = "Réglé", DateFactureClient = thisMonth, MontantTTC = 500 }
        ]);
        _factures.Setup(s => s.GetAllAsync(true, null)).ReturnsAsync(
        [
            new FactureClient { NumeroFactureClient = "AV001", IsAvoir = true, EtatReglement = "Réglé", DateFactureClient = thisMonth, MontantTTC = 120 }
        ]);
        AuthorizeAdmin();

        var cut = RenderComponent<Home>();
        cut.WaitForState(
            () => cut.Markup.Contains("CA du Mois"),
            TimeSpan.FromSeconds(5));

        // 500 (facture) - 120 (avoir) = 380
        Assert.Contains("380", cut.Markup);
    }

    // ── Stock Alerts ──────────────────────────────────────────────────────

    [Fact]
    public void StockAlert_RenderedWhenProductsBelowMinimum()
    {
        _produits.Setup(s => s.GetStockAlerteAsync()).ReturnsAsync(
        [
            new Produit { CodeProduit = "PR00001", DesignationProduit = "Stylo Bleu", Quantite = 2, StockMinimal = 10 }
        ]);
        AuthorizeAdmin();

        var cut = RenderComponent<Home>();
        cut.WaitForState(
            () => cut.Markup.Contains("Alertes Stock"),
            TimeSpan.FromSeconds(5));

        Assert.Contains("Stylo Bleu", cut.Markup);
    }

    [Fact]
    public void StockAlert_NotRendered_WhenNoProductsBelowMinimum()
    {
        AuthorizeAdmin();

        var cut = RenderComponent<Home>();
        cut.WaitForState(
            () => !cut.Markup.Contains("spinner-border"),
            TimeSpan.FromSeconds(5));

        Assert.DoesNotContain("Alertes Stock", cut.Markup);
    }

    // ── Recent Activity ───────────────────────────────────────────────────

    [Fact]
    public void NoFactures_ShowsNoActivityMessage()
    {
        AuthorizeAdmin();

        var cut = RenderComponent<Home>();
        cut.WaitForState(
            () => cut.Markup.Contains("Aucune activité"),
            TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void RecentFactures_DisplayedInActivity()
    {
        _factures.Setup(s => s.GetAllAsync(false, null)).ReturnsAsync(
        [
            new FactureClient
            {
                NumeroFactureClient = "FC202604001",
                EtatReglement       = "Non Réglé",
                DateFactureClient   = new DateTime(2026, 4, 21),
                MontantTTC          = 500.00,
                Client              = new Client { NomClient = "Client Test" }
            }
        ]);
        AuthorizeAdmin();

        var cut = RenderComponent<Home>();
        cut.WaitForState(
            () => cut.Markup.Contains("FC202604001"),
            TimeSpan.FromSeconds(5));

        Assert.Contains("Client Test", cut.Markup);
        Assert.Contains("FC202604001", cut.Markup);
    }

    // ── Page publique : section Tarifs ────────────────────────────────────

    private IRenderedComponent<Home> RenderAnonymous()
    {
        _currentUser.Setup(s => s.IsAuthenticated).Returns(false);
        JSInterop.Mode = JSRuntimeMode.Loose; // la page publique déclenche une animation JS
        this.AddTestAuthorization();
        return RenderComponent<Home>();
    }

    // ── Tableau de bord : rappel de fiche entreprise incomplète ───────────

    [Fact]
    public void Dashboard_WhenAdminAndFicheIncomplete_ShouldShowBannerLinkingToTheFiche()
    {
        // Arrange
        _currentUser.Setup(s => s.IsAdmin).Returns(true);
        _entreprise.Setup(s => s.IsFicheIncompleteAsync()).ReturnsAsync(true);
        AuthorizeAdmin();

        // Act
        var cut = RenderComponent<Home>();

        // Assert
        cut.WaitForAssertion(() => Assert.Contains("Complétez la fiche de votre entreprise", cut.Markup));
        Assert.Contains(cut.FindAll("a"), a => a.GetAttribute("href") == "entreprise" && a.TextContent.Contains("Compléter la fiche"));
    }

    [Fact]
    public void Dashboard_WhenFicheComplete_ShouldNotShowBanner()
    {
        // Arrange
        _currentUser.Setup(s => s.IsAdmin).Returns(true);
        _entreprise.Setup(s => s.IsFicheIncompleteAsync()).ReturnsAsync(false);
        AuthorizeAdmin();

        // Act
        var cut = RenderComponent<Home>();

        // Assert
        Assert.DoesNotContain("Complétez la fiche de votre entreprise", cut.Markup);
    }

    [Fact]
    public void Dashboard_WhenNotAdmin_ShouldNotShowBannerNorQueryTheFiche()
    {
        // Arrange — un employé ne peut pas modifier la fiche : pas d'alerte pour lui.
        _currentUser.Setup(s => s.IsAdmin).Returns(false);
        _entreprise.Setup(s => s.IsFicheIncompleteAsync()).ReturnsAsync(true);
        AuthorizeAdmin();

        // Act
        var cut = RenderComponent<Home>();

        // Assert
        Assert.DoesNotContain("Complétez la fiche de votre entreprise", cut.Markup);
        _entreprise.Verify(s => s.IsFicheIncompleteAsync(), Times.Never);
    }

    // ── Page publique : trois onglets, prix HT ────────────────────────────

    private static void Cliquer(IRenderedComponent<Home> cut, string onglet)
        => cut.FindAll("#tarifs button").Single(b => b.TextContent == onglet).Click();

    [Fact]
    public void Tarifs_OnAnnualTab_ShouldShowNewPricesExcludingTaxTrialAndInitiation()
    {
        // Act
        var cut = RenderAnonymous();

        // Assert
        var tarifs = cut.Find("#tarifs").TextContent;
        Assert.Contains("350 DT HT / an", tarifs);
        Assert.Contains("590 DT HT / an", tarifs);
        Assert.Contains("Prix hors taxes, TVA en sus", tarifs);
        Assert.Contains("Le plus populaire", tarifs);
        Assert.Contains("30 jours gratuits", tarifs);
        Assert.Contains("Initiation de 2 h incluse", tarifs);
        Assert.DoesNotContain("390", tarifs);
        Assert.DoesNotContain("690", tarifs);
    }

    [Fact]
    public void Tarifs_WhenSwitchingToMonthly_ShouldShowMonthlyPricesAndCarryCycleInLinks()
    {
        // Arrange
        var cut = RenderAnonymous();

        // Act
        Cliquer(cut, "Mensuel");

        // Assert
        var tarifs = cut.Find("#tarifs");
        Assert.Contains("39 DT HT / mois", tarifs.TextContent);
        Assert.Contains("69 DT HT / mois", tarifs.TextContent);
        Assert.Contains("Flexible", tarifs.TextContent);
        Assert.Contains("Sans engagement", tarifs.TextContent);
        Assert.DoesNotContain("Le plus populaire", tarifs.TextContent);
        Assert.Contains(tarifs.QuerySelectorAll("a"), a => a.GetAttribute("href") == "/demande-abonnement?plan=Pro&cycle=Mensuel");
    }

    [Fact]
    public void Tarifs_OnDesktopTab_ShouldShowThreeFormulasWithWhatHappensAfterTheFirstYear()
    {
        // Arrange
        var cut = RenderAnonymous();

        // Act
        Cliquer(cut, "Application Desktop");

        // Assert
        var tarifs = cut.Find("#tarifs");
        var texte = tarifs.TextContent.Replace('\u202f', ' ').Replace('\u00a0', ' ');
        Assert.Contains("1 000 DT HT", texte);
        Assert.Contains("1 400 DT HT", texte);
        Assert.Contains("2 400 DT HT", texte);
        Assert.Contains("Recommandé", texte);
        Assert.Contains("Paiement unique, pas d'abonnement", texte);
        Assert.Contains("Garantie 2 mois", texte);
        Assert.Contains("Mises à jour non incluses", texte);
        Assert.Contains("Installation et initiation de 2 h incluses", texte);
        Assert.Contains("maintenance facultative à 350 DT HT / an", texte);
        Assert.Contains("Licences pour 3 postes Windows", texte);
        Assert.Contains("Poste supplémentaire : 500 DT HT", texte);
        // Les abonnements ne sont plus affichés sur cet onglet.
        Assert.DoesNotContain("30 jours gratuits, sans carte bancaire", cut.FindAll("#tarifs .pricing-grid").Single().TextContent);
        var liens = tarifs.QuerySelectorAll(".pricing-grid a").Select(a => a.GetAttribute("href")).ToList();
        Assert.Equal(["/demande-abonnement?plan=DesktopEssentiel", "/demande-abonnement?plan=DesktopSerenite", "/demande-abonnement?plan=DesktopEquipe"], liens);
    }

    [Fact]
    public void Tarifs_LinkUnderSubscriptions_ShouldOpenTheDesktopTab()
    {
        // Arrange
        var cut = RenderAnonymous();

        // Act
        Cliquer(cut, "Voir l'offre Desktop");

        // Assert
        Assert.Contains("Sérénité", cut.Find("#tarifs").TextContent);
    }

    [Fact]
    public void Tarifs_ShouldNoLongerShowTheFounderDiscount()
    {
        // Arrange — même si un code existait encore en base, la bannière n'est plus demandée.
        _abonnements.Setup(s => s.GetOffrePromoAsync(It.IsAny<string?>()))
            .ReturnsAsync((string? code) => string.IsNullOrEmpty(code) ? null : new OffrePromo(code, "Client Fondateur", 35, 10, 7));

        // Act
        var cut = RenderAnonymous();

        // Assert
        Assert.DoesNotContain("Fondateur", cut.Markup);
        Assert.DoesNotContain("places restantes", cut.Markup);
    }
}
