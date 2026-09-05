namespace Web_T4C_GestCom.Services;

/// <summary>
/// Turns a delete failure into a message a user can act on. AppDbContext disables cascade delete on
/// every foreign key (DeleteBehavior.Restrict), so deleting a Client/Fournisseur/Produit still
/// referenced elsewhere throws a DbUpdateException wrapping a raw SQL "REFERENCE constraint" error —
/// EF's own top-level message ("An error occurred while saving...") tells the user nothing useful.
/// </summary>
public static class DeleteErrorMessageHelper
{
    public static string Build(Exception ex, string friendlyMessage)
    {
        var message = FlattenExceptionMessages(ex);

        if (message.Contains("REFERENCE constraint", StringComparison.OrdinalIgnoreCase)
            || message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase)
            || message.Contains("DELETE statement conflicted", StringComparison.OrdinalIgnoreCase)
            // SQL Server running with a French locale/collation phrases the same errors differently
            // (e.g. "L'instruction DELETE est en conflit avec la contrainte REFERENCE ...").
            || message.Contains("contrainte REFERENCE", StringComparison.OrdinalIgnoreCase)
            || message.Contains("instruction DELETE est en conflit", StringComparison.OrdinalIgnoreCase))
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
