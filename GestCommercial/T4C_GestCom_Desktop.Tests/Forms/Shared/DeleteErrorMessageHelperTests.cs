using T4C_GestCom_Desktop.Forms.Shared;
using Xunit;

namespace T4C_GestCom_Desktop.Tests.Forms.Shared;

public class DeleteErrorMessageHelperTests
{
    private const string FriendlyMessage = "Ce client ne peut pas etre supprime car il est lie a des factures.";

    [Fact]
    public void Build_WhenMessageMentionsReferenceConstraint_ReturnsFriendlyMessage()
    {
        // Arrange
        var ex = new InvalidOperationException("The DELETE statement conflicted with the REFERENCE constraint \"FK_factureclient_client\".");

        // Act
        var result = DeleteErrorMessageHelper.Build(ex, FriendlyMessage);

        // Assert
        Assert.Equal(FriendlyMessage, result);
    }

    [Fact]
    public void Build_WhenMessageMentionsForeignKey_ReturnsFriendlyMessage()
    {
        // Arrange
        var ex = new InvalidOperationException("Cannot delete because of a FOREIGN KEY constraint.");

        // Act
        var result = DeleteErrorMessageHelper.Build(ex, FriendlyMessage);

        // Assert
        Assert.Equal(FriendlyMessage, result);
    }

    [Fact]
    public void Build_WhenConstraintKeywordIsOnlyInInnerException_StillReturnsFriendlyMessage()
    {
        // Arrange — mirrors real EF Core behavior: DbUpdateException's own Message is generic,
        // the actual SQL Server "REFERENCE constraint" text lives on the InnerException.
        var sqlLike = new InvalidOperationException("The DELETE statement conflicted with the REFERENCE constraint \"FK_x\".");
        var wrapper = new InvalidOperationException("An error occurred while saving the entity changes. See the inner exception for details.", sqlLike);

        // Act
        var result = DeleteErrorMessageHelper.Build(wrapper, FriendlyMessage);

        // Assert
        Assert.Equal(FriendlyMessage, result);
    }

    [Fact]
    public void Build_WhenUnrelatedException_ReturnsRawMessagePrefixedWithErreur()
    {
        // Arrange
        var ex = new InvalidOperationException("Client introuvable.");

        // Act
        var result = DeleteErrorMessageHelper.Build(ex, FriendlyMessage);

        // Assert
        Assert.Equal("Erreur : Client introuvable.", result);
    }

    [Fact]
    public void Build_WithNestedUnrelatedExceptions_FlattensAllMessagesWithArrow()
    {
        // Arrange
        var inner = new InvalidOperationException("connexion perdue");
        var outer = new InvalidOperationException("echec de sauvegarde", inner);

        // Act
        var result = DeleteErrorMessageHelper.Build(outer, FriendlyMessage);

        // Assert
        Assert.Equal("Erreur : echec de sauvegarde -> connexion perdue", result);
    }
}
