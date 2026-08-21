using Web_T4C_GestCom.Data.Models;

namespace T4C_GestCom_Desktop.Forms.Shared;

/// <summary>Formats the "who this document is for" detail lines shown under the party name on a printed document.</summary>
public static class PartyDetailsHelper
{
    public static List<string> ForClient(Client? client)
    {
        var lines = new List<string>();
        if (client is null) return lines;
        if (!string.IsNullOrWhiteSpace(client.MatriculeFiscale)) lines.Add($"MF : {client.MatriculeFiscale}");
        if (!string.IsNullOrWhiteSpace(client.Adresse)) lines.Add(client.Adresse);
        if (!string.IsNullOrWhiteSpace(client.Ville)) lines.Add($"{client.CodePostal} {client.Ville}".Trim());
        if (!string.IsNullOrWhiteSpace(client.Tel)) lines.Add($"Tél : {client.Tel}");
        return lines;
    }

    public static List<string> ForFournisseur(Fournisseur? fournisseur)
    {
        var lines = new List<string>();
        if (fournisseur is null) return lines;
        if (!string.IsNullOrWhiteSpace(fournisseur.MatriculeFiscale)) lines.Add($"MF : {fournisseur.MatriculeFiscale}");
        if (!string.IsNullOrWhiteSpace(fournisseur.Adresse)) lines.Add(fournisseur.Adresse);
        if (!string.IsNullOrWhiteSpace(fournisseur.Ville)) lines.Add($"{fournisseur.CodePostal} {fournisseur.Ville}".Trim());
        if (!string.IsNullOrWhiteSpace(fournisseur.Tel)) lines.Add($"Tél : {fournisseur.Tel}");
        return lines;
    }
}
