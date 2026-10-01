using Microsoft.EntityFrameworkCore;
using Web_GestCom.Services;
using Xunit;

namespace Web_GestCom.Tests.Services;

public class UserErrorMessageTests
{
    // Cas réel rencontré en test (2026-09-30) : message SQL complet affiché à l'utilisateur.
    private static DbUpdateException ForeignKeyViolationOnSave() =>
        new("An error occurred while saving the entity changes. See the inner exception for details.",
            new Exception("The MERGE statement conflicted with the FOREIGN KEY constraint \"FK_ligneDevisClient_produit_code_produit\". " +
                          "The conflict occurred in database \"GestCom\", table \"dbo.produit\", column 'code_produit'."));

    private static void ThrowBusinessError() => throw new InvalidOperationException("Stock insuffisant pour le produit P001.");

    [Fact]
    public void Build_WhenForeignKeyViolationOnSave_ShouldHideSqlDetailsAndShowReference()
    {
        // Arrange
        var ex = ForeignKeyViolationOnSave();

        // Act
        var result = UserErrorMessage.Build(ex, "ABC12345");

        // Assert
        Assert.StartsWith(UserErrorMessage.ReferenceIntrouvable, result);
        Assert.Contains("réf. ABC12345", result);
        Assert.DoesNotContain("FK_ligneDevisClient", result);
        Assert.DoesNotContain("MERGE", result);
    }

    [Fact]
    public void Build_WhenOtherDbUpdateException_ShouldShowGenericSaveProblem()
    {
        // Arrange
        var ex = new DbUpdateException("An error occurred while saving the entity changes.",
            new Exception("String or binary data would be truncated in table 'dbo.client'."));

        // Act
        var result = UserErrorMessage.Build(ex, "ABC12345");

        // Assert
        Assert.StartsWith(UserErrorMessage.ProblemeSauvegarde, result);
        Assert.DoesNotContain("truncated", result);
    }

    [Fact]
    public void Build_WhenBusinessExceptionThrownByApplicationCode_ShouldKeepItsMessage()
    {
        // Arrange
        var ex = Record.Exception(ThrowBusinessError)!;

        // Act
        var result = UserErrorMessage.Build(ex, "ABC12345");

        // Assert
        Assert.Equal("Stock insuffisant pour le produit P001.", result);
    }

    [Fact]
    public void Build_WhenInvalidOperationExceptionThrownByFramework_ShouldHideIt()
    {
        // Arrange — levée par la BCL (namespace System.*), comme EF Core lève ses erreurs de tracking.
        var ex = Record.Exception(() => new List<int>().First())!;

        // Act
        var result = UserErrorMessage.Build(ex, "ABC12345");

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
        Assert.StartsWith(UserErrorMessage.ErreurTechnique, result);
    }

    [Fact]
    public void Build_WhenConcurrencyConflict_ShouldKeepItsMessage()
    {
        // Arrange
        var ex = new ConcurrencyConflictException(new DbUpdateConcurrencyException());

        // Act
        var result = UserErrorMessage.Build(ex, "ABC12345");

        // Assert
        Assert.Equal(ex.Message, result);
    }
}
