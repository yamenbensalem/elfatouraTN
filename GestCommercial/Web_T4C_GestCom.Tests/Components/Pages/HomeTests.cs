using Bunit;
using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Web_T4C_GestCom.Components.Pages;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;
using Xunit;

namespace Web_T4C_GestCom.Tests.Components.Pages;

public sealed class HomeTests : TestContext
{
    private readonly Mock<IClientService>        _clients      = new();
    private readonly Mock<IProduitService>       _produits     = new();
    private readonly Mock<IFournisseurService>   _fournisseurs = new();
    private readonly Mock<IFactureClientService> _factures     = new();
    private readonly Mock<ICurrentUserService>   _currentUser  = new();
    private readonly Mock<IPermissionService>    _permissions  = new();

    public HomeTests()
    {
        Services.AddScoped(_ => _clients.Object);
        Services.AddScoped(_ => _produits.Object);
        Services.AddScoped(_ => _fournisseurs.Object);
        Services.AddScoped(_ => _factures.Object);
        Services.AddScoped(_ => _currentUser.Object);
        Services.AddSingleton(_permissions.Object);

        // Default: empty collections so each test only sets up what it needs
        _clients.Setup(s => s.GetAllAsync(null)).ReturnsAsync([]);
        _produits.Setup(s => s.GetAllAsync(null, null)).ReturnsAsync([]);
        _produits.Setup(s => s.GetStockAlerteAsync()).ReturnsAsync([]);
        _fournisseurs.Setup(s => s.GetAllAsync(null)).ReturnsAsync([]);
        _factures.Setup(s => s.GetAllAsync(false, null)).ReturnsAsync([]);
        _factures.Setup(s => s.GetAllAsync(true, null)).ReturnsAsync([]);
        _currentUser.Setup(s => s.Login).Returns("testuser");
    }

    private void AuthorizeAdmin()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("testuser");
        auth.SetRoles("Admin");
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
}
