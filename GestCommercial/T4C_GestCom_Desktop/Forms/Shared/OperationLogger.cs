using Serilog;
using ILogger = Serilog.ILogger;

namespace T4C_GestCom_Desktop.Forms.Shared;

/// <summary>
/// One-line wrappers for the log shape repeated across every List/Edit screen (chargement,
/// suppression, clonage, enregistrement, "introuvable", impression) — centralizes the wording so
/// it's consistent and only needs updating in one place. Screens still create their own
/// <c>Log.ForContext&lt;T&gt;()</c> so log lines carry the emitting class as <c>SourceContext</c>.
/// </summary>
internal static class OperationLogger
{
    public static void DebugLoadingList(this ILogger logger, string entite, string? detail = null) =>
        logger.Debug(
            detail is null ? "Chargement de la liste des {Entite}." : "Chargement de la liste des {Entite} ({Detail}).",
            entite, detail);

    public static void DebugListLoaded(this ILogger logger, string entite, int count) =>
        logger.Debug("Liste des {Entite} chargée : {Count} résultats.", entite, count);

    public static void ErrorListLoadFailed(this ILogger logger, Exception ex, string entite) =>
        logger.Error(ex, "Échec du chargement de la liste des {Entite}.", entite);

    public static void DebugDeleting(this ILogger logger, string entite, string? code) =>
        logger.Debug("Suppression : {Entite} {Code}.", entite, code);

    public static void DebugDeleted(this ILogger logger, string entite, string? code) =>
        logger.Debug("{Entite} {Code} supprimé(e).", entite, code);

    public static void WarningDeleteFailed(this ILogger logger, Exception ex, string entite, string? code) =>
        logger.Warning(ex, "Échec de la suppression : {Entite} {Code}.", entite, code);

    public static void DebugCloning(this ILogger logger, string entite, string? numero) =>
        logger.Debug("Clonage : {Entite} {Numero}.", entite, numero);

    public static void DebugCloned(this ILogger logger, string entite, string? numero, string? nouveauNumero) =>
        logger.Debug("{Entite} {Numero} clonée en {NouveauNumero}.", entite, numero, nouveauNumero);

    public static void ErrorCloneFailed(this ILogger logger, Exception ex, string entite, string? numero) =>
        logger.Error(ex, "Échec du clonage : {Entite} {Numero}.", entite, numero);

    public static void DebugSaving(this ILogger logger, string entite, string? numero, bool isNew, int? lignes = null) =>
        logger.Debug(
            lignes is null
                ? "Enregistrement : {Entite} {Numero} (nouveau={IsNew})."
                : "Enregistrement : {Entite} {Numero} (nouveau={IsNew}, {Lignes} lignes).",
            entite, numero, isNew, lignes);

    public static void DebugSaved(this ILogger logger, string entite, string? numero) =>
        logger.Debug("{Entite} {Numero} enregistré(e).", entite, numero);

    public static void ErrorSaveFailed(this ILogger logger, Exception ex, string entite, string? numero) =>
        logger.Error(ex, "Échec de l'enregistrement : {Entite} {Numero}.", entite, numero);

    public static void WarningNotFound(this ILogger logger, string entite, string? id) =>
        logger.Warning("{Entite} {Id} introuvable à l'ouverture de l'éditeur.", entite, id);

    public static void ErrorPrintFailed(this ILogger logger, Exception ex, string entite, string? numero) =>
        logger.Error(ex, "Échec de l'impression : {Entite} {Numero}.", entite, numero);

    public static void DebugAddingReglement(this ILogger logger, string entite, string? numero, double montant) =>
        logger.Debug("Ajout d'un règlement de {Montant} sur {Entite} {Numero}.", montant, entite, numero);

    public static void DebugReglementAdded(this ILogger logger, string entite, string? numero) =>
        logger.Debug("Règlement ajouté sur {Entite} {Numero}.", entite, numero);

    public static void ErrorReglementFailed(this ILogger logger, Exception ex, string entite, string? numero) =>
        logger.Error(ex, "Échec de l'ajout du règlement sur {Entite} {Numero}.", entite, numero);
}
