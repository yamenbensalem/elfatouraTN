using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Ligne de commande d'achat
/// </summary>
public class LigneCommandeAchat : BaseEntity
{
    public int Id { get; set; }
    public int NumeroLigne { get; set; }
    public string NumeroCommande { get; set; } = string.Empty;
    public string CodeProduit { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public decimal Quantite { get; set; }
    public decimal QuantiteRecue { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal Remise { get; set; }
    public decimal MontantHT { get; set; }
    public decimal TauxTVA { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }

    // Navigation properties
    public CommandeAchat? CommandeAchat { get; set; }
    public Produit? Produit { get; set; }
}
