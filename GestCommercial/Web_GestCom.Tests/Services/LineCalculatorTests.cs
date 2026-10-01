using Web_GestCom.Services;
using Xunit;

namespace Web_GestCom.Tests.Services;

public class LineCalculatorTests
{
    [Fact]
    public void EnsureAllLinesHaveProduct_WhenEveryLineHasProduct_ShouldNotThrow()
    {
        // Arrange
        var codes = new[] { "PR00001", "PR00002" };

        // Act
        var ex = Record.Exception(() => LineCalculator.EnsureAllLinesHaveProduct(codes, c => c));

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void EnsureAllLinesHaveProduct_WhenOneLineEmpty_ShouldNameThatLine()
    {
        // Arrange
        var codes = new[] { "PR00001", "" };

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => LineCalculator.EnsureAllLinesHaveProduct(codes, c => c));

        // Assert
        Assert.Equal("La ligne 2 n'a pas de produit sélectionné. Choisissez un produit ou supprimez la ligne.", ex.Message);
    }

    [Fact]
    public void EnsureAllLinesHaveProduct_WhenSeveralLinesEmptyOrNull_ShouldNameAllOfThem()
    {
        // Arrange
        var codes = new[] { " ", "PR00001", null };

        // Act
        var ex = Assert.Throws<InvalidOperationException>(() => LineCalculator.EnsureAllLinesHaveProduct(codes, c => c));

        // Assert
        Assert.StartsWith("Les lignes 1, 3 n'ont pas de produit sélectionné.", ex.Message);
    }

    [Fact]
    public void EnsureAllLinesHaveProduct_ThrownMessage_ShouldBeShownAsIsToUser()
    {
        // Arrange
        var ex = Record.Exception(() => LineCalculator.EnsureAllLinesHaveProduct(new[] { "" }, c => c))!;

        // Act
        var message = UserErrorMessage.Build(ex, "ABC12345");

        // Assert
        Assert.Equal(ex.Message, message);
    }

    private readonly record struct Ligne(double Quantite, double PrixUnitaire);

    [Fact]
    public void EnsureNoNegativeAmounts_AllPositive_DoesNotThrow()
    {
        var lignes = new[] { new Ligne(2, 100), new Ligne(1, 50) };

        var ex = Record.Exception(() => LineCalculator.EnsureNoNegativeAmounts(lignes, l => l.Quantite, l => l.PrixUnitaire));

        Assert.Null(ex);
    }

    [Fact]
    public void EnsureNoNegativeAmounts_NegativeQuantite_Throws()
    {
        var lignes = new[] { new Ligne(-1, 100) };

        Assert.Throws<InvalidOperationException>(
            () => LineCalculator.EnsureNoNegativeAmounts(lignes, l => l.Quantite, l => l.PrixUnitaire));
    }

    [Fact]
    public void EnsureNoNegativeAmounts_NegativePrixUnitaire_Throws()
    {
        var lignes = new[] { new Ligne(1, -50) };

        Assert.Throws<InvalidOperationException>(
            () => LineCalculator.EnsureNoNegativeAmounts(lignes, l => l.Quantite, l => l.PrixUnitaire));
    }

    [Fact]
    public void EnsureNoNegativeAmounts_ZeroValues_DoesNotThrow()
    {
        var lignes = new[] { new Ligne(0, 0) };

        var ex = Record.Exception(() => LineCalculator.EnsureNoNegativeAmounts(lignes, l => l.Quantite, l => l.PrixUnitaire));

        Assert.Null(ex);
    }

    [Fact]
    public void EnsureNoNegativeAmounts_EmptyList_DoesNotThrow()
    {
        var lignes = Array.Empty<Ligne>();

        var ex = Record.Exception(() => LineCalculator.EnsureNoNegativeAmounts(lignes, l => l.Quantite, l => l.PrixUnitaire));

        Assert.Null(ex);
    }
}
