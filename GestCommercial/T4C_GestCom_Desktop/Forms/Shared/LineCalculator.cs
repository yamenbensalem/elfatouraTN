namespace T4C_GestCom_Desktop.Forms.Shared;

/// <summary>
/// Lightweight row for a Produit ComboBox DataSource. A named type instead of an anonymous
/// `new { p.CodeProduit, p.DesignationProduit }` deliberately — WinForms' DisplayMember/ValueMember
/// binding resolves "CodeProduit"/"DesignationProduit" by reflection at runtime, and an anonymous
/// type's compiler-generated properties get renamed by obfuscation tools (breaking that lookup)
/// where a named, explicitly-excludable type does not.
/// </summary>
public sealed record ProduitOption(string CodeProduit, string DesignationProduit);

/// <summary>
/// The line/total math shared by every document editor (Devis, Commandes, Bons, Factures) —
/// extracted out of ProductLinesEditor and FactureClientEditForm (which duplicated it inline)
/// so it has one implementation and can be unit tested directly, since the WinForms grids that
/// use it aren't practically testable themselves. Mirrors RecalculerLigne/RecalculerTotaux in the
/// matching .razor pages exactly (same rounding, same order of operations).
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
    /// pass Fodec = 0 on every line and remisePercent = 0 for document types that don't carry them
    /// (matches the includeFodec/includeRemise flags on ProductLinesEditor).
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
