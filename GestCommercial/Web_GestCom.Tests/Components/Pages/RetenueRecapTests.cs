using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Web_GestCom.Components.Pages.Rapports;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;
using Web_GestCom.Tests.Helpers;
using Xunit;

namespace Web_GestCom.Tests.Components.Pages;

public sealed class RetenueRecapTests : TestContext
{
    // Reproduit le cas signalé le 2026-09-30 : 2 factures client + 1 facture fournisseur avec retenue,
    // affichées avec un seul "Total" (17900 / 268,5) qui mélangeait retenues subies et opérées.
    private AppDbContext SeedVentesEtAchat()
    {
        var db = DbContextFactory.Create();
        db.Clients.Add(new Client { CodeClient = "CL00001", NomClient = "Test", CodeDevise = 1 });
        db.Fournisseurs.Add(new Fournisseur { CodeFournisseur = "FO00001", NomFournisseur = "Test" });
        db.FacturesClient.AddRange(
            new FactureClient { NumeroFactureClient = "FC202609001", CodeClient = "CL00001", DateFactureClient = DateTime.Today, MontantHT = 4950, MontantRetenue = 74.25 },
            new FactureClient { NumeroFactureClient = "FC202609002", CodeClient = "CL00001", DateFactureClient = DateTime.Today, MontantHT = 4950, MontantRetenue = 74.25 });
        db.FacturesFournisseur.Add(
            new FactureFournisseur { NumeroFactureFournisseur = "FF202609001", CodeFournisseur = "FO00001", DateFactureFournisseur = DateTime.Today, MontantHT = 8000, MontantRetenue = 120 });
        db.SaveChanges();
        return db;
    }

    [Fact]
    public void Render_WhenVentesAndAchats_ShouldShowSeparateTotalsAndNoGrandTotal()
    {
        // Arrange
        Services.AddSingleton(SeedVentesEtAchat());
        JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = RenderComponent<RetenueRecap>();
        cut.WaitForState(() => cut.Markup.Contains("FF202609001"), TimeSpan.FromSeconds(5));

        // Assert
        var totaux = cut.FindAll("tfoot tr").Select(tr => tr.TextContent).ToList();
        Assert.Equal(2, totaux.Count);
        Assert.Contains(totaux, t => t.Contains("retenues subies") && t.Contains("9900") && t.Contains("148,5"));
        Assert.Contains(totaux, t => t.Contains("retenues opérées") && t.Contains("8000") && t.Contains("120"));
        Assert.DoesNotContain("17900", cut.Markup);
        Assert.DoesNotContain("268,5", cut.Markup);
    }

    [Fact]
    public void Render_WhenAvoirClient_ShouldDeductItFromVentesTotal()
    {
        // Arrange — avoir stocké en positif, comme le génère FactureClientService.
        var db = SeedVentesEtAchat();
        db.FacturesClient.Add(new FactureClient
        {
            NumeroFactureClient = "AV202609001", CodeClient = "CL00001", DateFactureClient = DateTime.Today,
            MontantHT = 4950, MontantRetenue = 74.25, IsAvoir = true
        });
        db.SaveChanges();
        Services.AddSingleton(db);
        JSInterop.Mode = JSRuntimeMode.Loose;

        // Act
        var cut = RenderComponent<RetenueRecap>();
        cut.WaitForState(() => cut.Markup.Contains("AV202609001"), TimeSpan.FromSeconds(5));

        // Assert
        var ligneAvoir = cut.FindAll("tbody tr").Single(tr => tr.TextContent.Contains("AV202609001")).TextContent;
        Assert.Contains("Avoir vente", ligneAvoir);
        Assert.Contains("-4950", ligneAvoir);
        Assert.Contains("-74,25", ligneAvoir);
        var totalVentes = cut.FindAll("tfoot tr").Single(tr => tr.TextContent.Contains("retenues subies")).TextContent;
        Assert.Contains("avoirs déduits", totalVentes);
        Assert.Contains("4950", totalVentes);
        Assert.Contains("74,25", totalVentes);
        Assert.DoesNotContain("14850", cut.Markup);
    }
}
