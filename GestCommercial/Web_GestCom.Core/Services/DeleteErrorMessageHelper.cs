namespace Web_GestCom.Services;

/// <summary>
/// Turns a delete failure into a message a user can act on. AppDbContext disables cascade delete on
/// every foreign key (DeleteBehavior.Restrict), so deleting a Client/Fournisseur/Produit still
/// referenced elsewhere throws a DbUpdateException wrapping a raw SQL "REFERENCE constraint" error —
/// EF's own top-level message ("An error occurred while saving...") tells the user nothing useful.
/// </summary>
public static class DeleteErrorMessageHelper
{
    /// <summary>
    /// Version Desktop : pas de journal consultable ni de référence côté poste client, donc une
    /// erreur qui n'est pas "encore utilisé ailleurs" est affichée en entier.
    /// </summary>
    public static string Build(Exception ex, string friendlyMessage)
        => IsReferenceConstraint(ex) ? friendlyMessage : $"Erreur : {FlattenExceptionMessages(ex)}";

    /// <summary>
    /// Version Web (Notification.razor) : jamais le message brut (SQL, EF...) à l'écran — il est
    /// dans les logs, retrouvable par <paramref name="reference"/>.
    /// </summary>
    public static string Build(Exception ex, string friendlyMessage, string reference)
        => IsReferenceConstraint(ex) ? friendlyMessage : UserErrorMessage.Build(ex, reference);

    private static bool IsReferenceConstraint(Exception ex)
    {
        var message = FlattenExceptionMessages(ex);

        return message.Contains("REFERENCE constraint", StringComparison.OrdinalIgnoreCase)
            || message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase)
            || message.Contains("DELETE statement conflicted", StringComparison.OrdinalIgnoreCase)
            // SQL Server running with a French locale/collation phrases the same errors differently
            // (e.g. "L'instruction DELETE est en conflit avec la contrainte REFERENCE ...").
            || message.Contains("contrainte REFERENCE", StringComparison.OrdinalIgnoreCase)
            || message.Contains("instruction DELETE est en conflit", StringComparison.OrdinalIgnoreCase);
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
