namespace T4C_GestCom_Desktop.Forms.Shared;

/// <summary>
/// Turns a delete failure into a message a user can act on. AppDbContext disables cascade delete on
/// every foreign key (DeleteBehavior.Restrict), so deleting a Client/Fournisseur/Produit still
/// referenced elsewhere throws a DbUpdateException wrapping a raw SQL "REFERENCE constraint" error —
/// EF's own top-level message ("An error occurred while saving...") tells the user nothing useful.
/// Mirrors the BuildDeleteErrorMessage/FlattenExceptionMessages pair duplicated across
/// ClientsList.razor, FournisseursList.razor and ProduitsList.razor.
/// </summary>
public static class DeleteErrorMessageHelper
{
    public static string Build(Exception ex, string friendlyMessage)
    {
        var message = FlattenExceptionMessages(ex);

        if (message.Contains("REFERENCE constraint", StringComparison.OrdinalIgnoreCase)
            || message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase)
            || message.Contains("DELETE statement conflicted", StringComparison.OrdinalIgnoreCase))
        {
            return friendlyMessage;
        }

        return $"Erreur : {message}";
    }

    private static string FlattenExceptionMessages(Exception ex)
    {
        var messages = new List<string>();
        var current = ex;

        while (current is not null)
        {
            if (!string.IsNullOrWhiteSpace(current.Message))
                messages.Add(current.Message.Trim());
            current = current.InnerException;
        }

        return string.Join(" -> ", messages);
    }
}
