namespace Web_T4C_GestCom.Services;

/// <summary>
/// The line/total math shared by every document editor (Devis, Commandes, Bons, Factures) — one
/// implementation used by both Web_T4C_GestCom's Razor pages and T4C_GestCom_Desktop's WinForms
/// grids, instead of each reimplementing the same rounding/order of operations independently.
/// </summary>
public static class LineCalculator
{
    /// <summary>Montant HT for one line: quantité × prix unitaire, net of the line discount.</summary>
    public static double LineMontantHT(double quantite, double prixUnitaire, double remise)
        => Math.Round(quantite * prixUnitaire * (1 - remise / 100), 3);

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
