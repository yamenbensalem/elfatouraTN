using Bunit;
using Web_T4C_GestCom.Components.Shared;
using Web_T4C_GestCom.Data.Models;
using Xunit;

namespace Web_T4C_GestCom.Tests.Components.Shared;

public sealed class PrintDocHeaderTests : TestContext
{
    private static Entreprise MakeEntreprise(string? pathLogo = null) => new()
    {
        CodeEntreprise  = "ENT001",
        NomEntreprise   = "Tech Solutions SARL",
        MatriculeFiscale = "1234567/A/M/000",
        Adresse         = "12 Rue de la République",
        CodePostal      = "1001",
        Ville           = "Tunis",
        Tel             = "+216 71 000 000",
        Email           = "contact@techsolutions.tn",
        PathLogo        = pathLogo
    };

    // ── Company Information ───────────────────────────────────────────────

    [Fact]
    public void RendersCompanyName()
    {
        var cut = RenderComponent<PrintDocHeader>(p => p
            .Add(x => x.Entreprise, MakeEntreprise())
            .Add(x => x.DocType, "Facture")
            .Add(x => x.Numero, "FC202604001")
            .Add(x => x.Date, new DateTime(2026, 4, 21)));

        Assert.Contains("Tech Solutions SARL", cut.Markup);
    }

    [Fact]
    public void RendersMatriculeFiscale()
    {
        var cut = RenderComponent<PrintDocHeader>(p => p
            .Add(x => x.Entreprise, MakeEntreprise())
            .Add(x => x.DocType, "Facture")
            .Add(x => x.Numero, "FC202604001")
            .Add(x => x.Date, new DateTime(2026, 4, 21)));

        Assert.Contains("1234567/A/M/000", cut.Markup);
    }

    [Fact]
    public void RendersContactDetails()
    {
        var cut = RenderComponent<PrintDocHeader>(p => p
            .Add(x => x.Entreprise, MakeEntreprise())
            .Add(x => x.DocType, "Facture")
            .Add(x => x.Numero, "FC202604001")
            .Add(x => x.Date, new DateTime(2026, 4, 21)));

        Assert.Contains("+216 71 000 000", cut.Markup);
        Assert.Contains("contact@techsolutions.tn", cut.Markup);
    }

    // ── Document Information ──────────────────────────────────────────────

    [Fact]
    public void RendersDocTypeAndNumero()
    {
        var cut = RenderComponent<PrintDocHeader>(p => p
            .Add(x => x.Entreprise, MakeEntreprise())
            .Add(x => x.DocType, "Bon de Livraison")
            .Add(x => x.Numero, "BL202604042")
            .Add(x => x.Date, new DateTime(2026, 4, 21)));

        Assert.Contains("Bon de Livraison", cut.Markup);
        Assert.Contains("BL202604042", cut.Markup);
    }

    [Fact]
    public void RendersDateInFrenchFormat()
    {
        var cut = RenderComponent<PrintDocHeader>(p => p
            .Add(x => x.Entreprise, MakeEntreprise())
            .Add(x => x.DocType, "Devis")
            .Add(x => x.Numero, "DV202604001")
            .Add(x => x.Date, new DateTime(2026, 4, 21)));

        Assert.Contains("21/04/2026", cut.Markup);
    }

    [Fact]
    public void RendersEtat_WhenProvided()
    {
        var cut = RenderComponent<PrintDocHeader>(p => p
            .Add(x => x.Entreprise, MakeEntreprise())
            .Add(x => x.DocType, "Facture")
            .Add(x => x.Numero, "FC202604001")
            .Add(x => x.Date, new DateTime(2026, 4, 21))
            .Add(x => x.Etat, "Réglé"));

        Assert.Contains("Réglé", cut.Markup);
    }

    [Fact]
    public void DoesNotRenderEtat_WhenNull()
    {
        var cut = RenderComponent<PrintDocHeader>(p => p
            .Add(x => x.Entreprise, MakeEntreprise())
            .Add(x => x.DocType, "Facture")
            .Add(x => x.Numero, "FC202604001")
            .Add(x => x.Date, new DateTime(2026, 4, 21))
            .Add(x => x.Etat, (string?)null));

        Assert.DoesNotContain("État :", cut.Markup);
    }

    // ── Logo Path Normalisation ───────────────────────────────────────────

    [Fact]
    public void LogoPath_TildePrefix_IsStripped()
    {
        var cut = RenderComponent<PrintDocHeader>(p => p
            .Add(x => x.Entreprise, MakeEntreprise("~/images/logo.png"))
            .Add(x => x.DocType, "Facture")
            .Add(x => x.Numero, "FC202604001")
            .Add(x => x.Date, DateTime.Today));

        var img = cut.Find("img");
        Assert.Equal("images/logo.png", img.GetAttribute("src"));
    }

    [Fact]
    public void LogoPath_DotSlashPrefix_IsStripped()
    {
        var cut = RenderComponent<PrintDocHeader>(p => p
            .Add(x => x.Entreprise, MakeEntreprise("./logo.png"))
            .Add(x => x.DocType, "Facture")
            .Add(x => x.Numero, "FC202604001")
            .Add(x => x.Date, DateTime.Today));

        var img = cut.Find("img");
        Assert.Equal("logo.png", img.GetAttribute("src"));
    }

    [Fact]
    public void LogoPath_NullEntrepriseLogo_UsesDefault()
    {
        var cut = RenderComponent<PrintDocHeader>(p => p
            .Add(x => x.Entreprise, MakeEntreprise(pathLogo: null))
            .Add(x => x.DocType, "Facture")
            .Add(x => x.Numero, "FC202604001")
            .Add(x => x.Date, DateTime.Today));

        var img = cut.Find("img");
        Assert.Equal("logoApp.png", img.GetAttribute("src"));
    }
}
