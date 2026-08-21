using T4C_GestCom_Desktop.Forms.Shared;
using Xunit;

namespace T4C_GestCom_Desktop.Tests.Forms.Shared;

public class PrintDocumentBuilderTests
{
    private static PrintDocumentModel MakeModel(
        string? note = null,
        IReadOnlyList<(string, string)>? headerRight = null,
        (string[] Headers, IReadOnlyList<string[]> Rows)? reglements = null,
        string partyName = "Client Test")
        => new(
            DocType: "FACTURE CLIENT",
            Numero: "FC2026080001",
            Date: new DateTime(2026, 8, 21),
            Etat: "Facture Ouverte",
            PartyLabel: "Facturé à",
            PartyName: partyName,
            PartyDetails: ["MF : 123456A", "Tunis"],
            HeaderRight: headerRight ?? [],
            ColumnHeaders: ["Désignation", "Qté", "Prix HT", "Montant HT"],
            Rows: [["Produit A", "2", "10.000", "20.000"]],
            Totals: [("Total HT", "20.000", false), ("Net à Payer", "23.800 TND", true)],
            Note: note,
            Reglements: reglements,
            EntrepriseFooter: "Ma Société SARL");

    [Fact]
    public void BuildHtml_IncludesDocTypeAndNumero()
    {
        // Arrange
        var model = MakeModel();

        // Act
        var html = PrintDocumentBuilder.BuildHtml(model);

        // Assert
        Assert.Contains("FACTURE CLIENT", html);
        Assert.Contains("FC2026080001", html);
    }

    [Fact]
    public void BuildHtml_IncludesLineRowsAndTotals()
    {
        // Arrange
        var model = MakeModel();

        // Act
        var html = PrintDocumentBuilder.BuildHtml(model);

        // Assert — "Payer" (not "à") because WebUtility.HtmlEncode turns accented characters into
        // numeric entities, so the literal accented substring never appears verbatim in the markup.
        Assert.Contains("Produit A", html);
        Assert.Contains("20.000", html);
        Assert.Contains("Payer", html);
        Assert.Contains("23.800 TND", html);
    }

    [Fact]
    public void BuildHtml_EscapesHtmlInPartyName()
    {
        // Arrange — a client/produit name containing HTML-significant characters must not break the markup
        var model = MakeModel(partyName: "Client <script>alert('x')</script> & Co");

        // Act
        var html = PrintDocumentBuilder.BuildHtml(model);

        // Assert
        Assert.DoesNotContain("<script>", html);
        Assert.Contains("&lt;script&gt;", html);
        Assert.Contains("&amp;", html);
    }

    [Fact]
    public void BuildHtml_WithoutNote_OmitsNoteBlock()
    {
        // Arrange
        var model = MakeModel(note: null);

        // Act
        var html = PrintDocumentBuilder.BuildHtml(model);

        // Assert
        Assert.DoesNotContain("Note :", html);
    }

    [Fact]
    public void BuildHtml_WithNote_IncludesNoteText()
    {
        // Arrange
        var model = MakeModel(note: "Livraison urgente");

        // Act
        var html = PrintDocumentBuilder.BuildHtml(model);

        // Assert
        Assert.Contains("Note :", html);
        Assert.Contains("Livraison urgente", html);
    }

    [Fact]
    public void BuildHtml_WithoutReglements_OmitsReglementsSection()
    {
        // Arrange
        var model = MakeModel(reglements: null);

        // Act
        var html = PrintDocumentBuilder.BuildHtml(model);

        // Assert
        Assert.DoesNotContain("Règlements", html);
    }

    [Fact]
    public void BuildHtml_WithReglements_IncludesReglementRows()
    {
        // Arrange
        var reglements = (
            Headers: new[] { "Date", "Mode", "Référence", "Montant" },
            Rows: (IReadOnlyList<string[]>)new List<string[]> { new[] { "21/08/2026", "Chèque", "CH-001", "23.800" } });
        var model = MakeModel(reglements: reglements);

        // Act
        var html = PrintDocumentBuilder.BuildHtml(model);

        // Assert
        Assert.Contains("Règlements", html);
        Assert.Contains("CH-001", html);
    }

    [Fact]
    public void BuildHtml_WithHeaderRight_IncludesEachLabelAndValue()
    {
        // Arrange
        var model = MakeModel(headerRight: [("État règlement", "Partiellement Réglé")]);

        // Act
        var html = PrintDocumentBuilder.BuildHtml(model);

        // Assert — accent-free substrings only (see BuildHtml_IncludesLineRowsAndTotals for why).
        Assert.Contains("glement", html);
        Assert.Contains("Partiellement", html);
    }
}
