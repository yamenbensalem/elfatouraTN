using T4C_GestCom_Desktop.Forms.Shared;
using Xunit;

namespace T4C_GestCom_Desktop.Tests.Forms.Shared;

public class LineCalculatorTests
{
    // ── LineMontantHT ────────────────────────────────────────────────────────

    [Fact]
    public void LineMontantHT_NoRemise_MultipliesQuantiteByPrixUnitaire()
    {
        // Arrange
        double quantite = 3, prixUnitaire = 10.5, remise = 0;

        // Act
        var result = LineCalculator.LineMontantHT(quantite, prixUnitaire, remise);

        // Assert
        Assert.Equal(31.5, result);
    }

    [Fact]
    public void LineMontantHT_WithRemise_AppliesDiscountBeforeRounding()
    {
        // Arrange
        double quantite = 2, prixUnitaire = 100, remise = 10;

        // Act
        var result = LineCalculator.LineMontantHT(quantite, prixUnitaire, remise);

        // Assert
        Assert.Equal(180.0, result);
    }

    [Fact]
    public void LineMontantHT_RoundsToThreeDecimals()
    {
        // Arrange
        double quantite = 1, prixUnitaire = 10.0 / 3.0, remise = 0;

        // Act
        var result = LineCalculator.LineMontantHT(quantite, prixUnitaire, remise);

        // Assert
        Assert.Equal(3.333, result);
    }

    [Fact]
    public void LineMontantHT_FullRemise_ReturnsZero()
    {
        // Arrange & Act
        var result = LineCalculator.LineMontantHT(5, 20, 100);

        // Assert
        Assert.Equal(0, result);
    }

    // ── CalculateDocumentTotals ─────────────────────────────────────────────

    [Fact]
    public void CalculateDocumentTotals_NoLines_ReturnsAllZero()
    {
        // Arrange
        var lines = Array.Empty<LineCalculator.LineAmounts>();

        // Act
        var totals = LineCalculator.CalculateDocumentTotals(lines, remisePercent: 0);

        // Assert
        Assert.Equal(0, totals.TotalHT);
        Assert.Equal(0, totals.TotalFodec);
        Assert.Equal(0, totals.TotalTva);
        Assert.Equal(0, totals.TotalTTC);
    }

    [Fact]
    public void CalculateDocumentTotals_SingleLine_SumsHTAndAppliesTvaPercent()
    {
        // Arrange — mirrors a Devis/CommandeVente line: no FODEC, 19% TVA
        var lines = new[] { new LineCalculator.LineAmounts(MontantHT: 100, Tva: 19, Fodec: 0) };

        // Act
        var totals = LineCalculator.CalculateDocumentTotals(lines, remisePercent: 0);

        // Assert
        Assert.Equal(100, totals.TotalHT);
        Assert.Equal(0, totals.TotalFodec);
        Assert.Equal(19, totals.TotalTva);
        Assert.Equal(119, totals.TotalTTC);
    }

    [Fact]
    public void CalculateDocumentTotals_WithHeaderRemise_DeductsFromHTBeforeAddingTaxes()
    {
        // Arrange
        var lines = new[] { new LineCalculator.LineAmounts(MontantHT: 200, Tva: 19, Fodec: 0) };

        // Act — 10% header remise on a 200 HT / 38 TVA document
        var totals = LineCalculator.CalculateDocumentTotals(lines, remisePercent: 10);

        // Assert: 200 - 20 (remise) + 0 (fodec) + 38 (tva) = 218
        Assert.Equal(200, totals.TotalHT);
        Assert.Equal(38, totals.TotalTva);
        Assert.Equal(218, totals.TotalTTC);
    }

    [Fact]
    public void CalculateDocumentTotals_WithFodec_AddsFodecOnTopOfTva()
    {
        // Arrange — mirrors a Facture Client line carrying FODEC
        var lines = new[] { new LineCalculator.LineAmounts(MontantHT: 100, Tva: 19, Fodec: 1) };

        // Act
        var totals = LineCalculator.CalculateDocumentTotals(lines, remisePercent: 0);

        // Assert: 100 HT, 1% fodec = 1, 19% tva = 19, TTC = 100 - 0 + 1 + 19 = 120
        Assert.Equal(1, totals.TotalFodec);
        Assert.Equal(19, totals.TotalTva);
        Assert.Equal(120, totals.TotalTTC);
    }

    [Fact]
    public void CalculateDocumentTotals_MultipleLines_SumsEachLineIndependently()
    {
        // Arrange — two lines with different TVA rates, matching per-line TVA aggregation in RecalculerTotaux
        var lines = new[]
        {
            new LineCalculator.LineAmounts(MontantHT: 100, Tva: 19, Fodec: 0),
            new LineCalculator.LineAmounts(MontantHT: 50, Tva: 7, Fodec: 0),
        };

        // Act
        var totals = LineCalculator.CalculateDocumentTotals(lines, remisePercent: 0);

        // Assert: HT = 150, TVA = 19 + 3.5 = 22.5, TTC = 172.5
        Assert.Equal(150, totals.TotalHT);
        Assert.Equal(22.5, totals.TotalTva);
        Assert.Equal(172.5, totals.TotalTTC);
    }
}
