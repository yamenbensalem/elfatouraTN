using Web_GestCom.Services;
using Xunit;

namespace Web_GestCom.Tests.Services;

public class DeleteErrorMessageHelperTests
{
    [Fact]
    public void Build_EnglishReferenceConstraintMessage_ReturnsFriendlyMessage()
    {
        var ex = new Exception("An error occurred while saving the entity changes.",
            new Exception("The DELETE statement conflicted with the REFERENCE constraint \"FK_x\"."));

        var result = DeleteErrorMessageHelper.Build(ex, "Impossible de supprimer : utilisé ailleurs.");

        Assert.Equal("Impossible de supprimer : utilisé ailleurs.", result);
    }

    [Fact]
    public void Build_FrenchReferenceConstraintMessage_ReturnsFriendlyMessage()
    {
        // SQL Server under a French locale/collation phrases this error differently from the
        // English "REFERENCE constraint" / "DELETE statement conflicted" wording.
        var ex = new Exception("An error occurred while saving the entity changes.",
            new Exception("L'instruction DELETE est en conflit avec la contrainte REFERENCE \"FK_produit_tvaproduit_code_tvaproduit\"."));

        var result = DeleteErrorMessageHelper.Build(ex, "Impossible de supprimer : utilisé ailleurs.");

        Assert.Equal("Impossible de supprimer : utilisé ailleurs.", result);
    }

    [Fact]
    public void Build_UnrelatedTechnicalException_ReturnsGenericMessageWithReference()
    {
        var ex = new NullReferenceException("Object reference not set to an instance of an object.");

        var result = DeleteErrorMessageHelper.Build(ex, "Impossible de supprimer : utilisé ailleurs.", "ABC12345");

        Assert.DoesNotContain("Object reference", result);
        Assert.StartsWith(UserErrorMessage.ErreurTechnique, result);
        Assert.Contains("réf. ABC12345", result);
    }
}
