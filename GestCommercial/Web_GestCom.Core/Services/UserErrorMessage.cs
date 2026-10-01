using Microsoft.EntityFrameworkCore;

namespace Web_GestCom.Services;

/// <summary>
/// Traduit une exception en message affichable à l'utilisateur. Les messages métier écrits pour
/// l'utilisateur (InvalidOperationException/UnauthorizedAccessException levées par notre propre code,
/// ConcurrencyConflictException) sont conservés tels quels ; toute erreur technique (SQL, EF Core,
/// NullReference...) est remplacée par un message générique — le détail complet (exception SQL
/// interne comprise) va uniquement dans les logs, retrouvable via la référence affichée.
/// </summary>
public static class UserErrorMessage
{
    public const string ProblemeSauvegarde =
        "Problème de sauvegarde : l'opération n'a pas pu être enregistrée.";

    public const string ReferenceIntrouvable =
        "Problème de sauvegarde : un élément utilisé (produit, client, fournisseur…) n'existe pas ou plus. Vérifiez vos saisies puis réessayez.";

    public const string ErreurTechnique =
        "Une erreur technique est survenue.";

    public static string Build(Exception ex, string? reference)
    {
        if (IsUserFacing(ex)) return ex.Message;

        var message = ex is DbUpdateException
            ? (ContainsForeignKeyViolation(ex) ? ReferenceIntrouvable : ProblemeSauvegarde)
            : ErreurTechnique;

        return reference is null
            ? message
            : $"{message} Si le problème persiste, contactez le support (réf. {reference}).";
    }

    /// <summary>
    /// Vrai pour les messages écrits à destination de l'utilisateur. InvalidOperationException est
    /// aussi levée par EF Core lui-même (ex. "The instance of entity type ... cannot be tracked") —
    /// on ne la considère métier que si elle a été levée depuis le code de l'application.
    /// </summary>
    public static bool IsUserFacing(Exception ex) => ex switch
    {
        ConcurrencyConflictException => true,
        InvalidOperationException or UnauthorizedAccessException => ThrownByApplicationCode(ex),
        _ => false
    };

    private static bool ThrownByApplicationCode(Exception ex)
        => ex.TargetSite?.DeclaringType?.Namespace?.StartsWith("Web_GestCom", StringComparison.Ordinal) == true;

    private static bool ContainsForeignKeyViolation(Exception ex)
    {
        for (var current = ex; current is not null; current = current.InnerException)
        {
            if (current.Message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
