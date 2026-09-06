using Web_GestCom.Services;
using Xunit;

namespace Web_GestCom.Tests.Services;

public class LineCalculatorTests
{
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
