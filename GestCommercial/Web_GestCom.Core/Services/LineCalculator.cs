namespace Web_GestCom.Services;

/// <summary>
/// The line/total math shared by every document editor (Devis, Commandes, Bons, Factures) — one
/// implementation used by both Web_GestCom's Razor pages and GestCom_Desktop's WinForms
/// grids, instead of each reimplementing the same rounding/order of operations independently.
/// </summary>
public static class LineCalculator
{
    /// <summary>Montant HT for one line: quantité × prix unitaire, net of the line discount.</summary>
    public static double LineMontantHT(double quantite, double prixUnitaire, double remise)
        => Math.Round(quantite * prixUnitaire * (1 - remise / 100), 3);

    /// <summary>
    /// Guard shared by every document service (Devis, Commandes, Bons, Factures) so a negative
    /// quantité or prix unitaire can never reach SaveChangesAsync, regardless of which UI (Web or
    /// Desktop) produced the line. Throws InvalidOperationException — the same "business rule
    /// violation" convention already used across these services (ex. "Facture introuvable").
    /// </summary>
    public static void EnsureNoNegativeAmounts<T>(IEnumerable<T> lignes, Func<T, double> quantite, Func<T, double> prixUnitaire)
    {
        if (lignes.Any(l => quantite(l) < 0 || prixUnitaire(l) < 0))
            throw new InvalidOperationException("La quantité et le prix unitaire d'une ligne ne peuvent pas être négatifs.");
    }

    /// <summary>
    /// Guard shared by every document service: a line left on "-- Produit --" in the UI has an empty
    /// CodeProduit, which reached SQL Server and failed on the FK to produit (real bug, 2026-09-30,
    /// "The MERGE statement conflicted with the FOREIGN KEY constraint FK_ligneDevisClient_produit...").
    /// Reject it up front with a message naming the offending line(s) (numbered from 1, as displayed).
    /// </summary>
    public static void EnsureAllLinesHaveProduct<T>(IEnumerable<T> lignes, Func<T, string?> codeProduit)
    {
        var numerosSansProduit = lignes
            .Select((ligne, index) => (ligne, numero: index + 1))
            .Where(x => string.IsNullOrWhiteSpace(codeProduit(x.ligne)))
            .Select(x => x.numero)
            .ToList();

        if (numerosSansProduit.Count == 0) return;

        var lignesTexte = numerosSansProduit.Count == 1
            ? $"La ligne {numerosSansProduit[0]} n'a"
            : $"Les lignes {string.Join(", ", numerosSansProduit)} n'ont";
        throw new InvalidOperationException(
            $"{lignesTexte} pas de produit sélectionné. Choisissez un produit ou supprimez la ligne.");
    }

    public readonly record struct LineAmounts(double MontantHT, double Tva, double Fodec);

    public readonly record struct DocumentTotals(double TotalHT, double TotalFodec, double TotalTva, double TotalTTC);

    /// <summary>
    /// Aggregates a document's lines into header totals. FODEC and header-level remise are optional —
    /// pass Fodec = 0 on every line and remisePercent = 0 for document types that don't carry them.
    /// </summary>
    public static DocumentTotals CalculateDocumentTotals(IEnumerable<LineAmounts> lines, double remisePercent)
    {
        double totalHT = 0, totalFodec = 0, totalTva = 0;
        foreach (var line in lines)
        {
            totalHT += line.MontantHT;
            totalFodec += line.Fodec * line.MontantHT / 100;
            totalTva += line.Tva * line.MontantHT / 100;
        }

        totalHT = Math.Round(totalHT, 3);
        totalFodec = Math.Round(totalFodec, 3);
        totalTva = Math.Round(totalTva, 3);

        var remiseMontant = totalHT * remisePercent / 100;
        var totalTTC = Math.Round(totalHT - remiseMontant + totalFodec + totalTva, 3);

        return new DocumentTotals(totalHT, totalFodec, totalTva, totalTTC);
    }
}
