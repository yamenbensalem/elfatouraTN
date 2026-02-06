using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Ligne de bon de livraison
/// </summary>
public class LigneBonLivraison : BaseEntity
{
    public int Id { get; set; }
    public int NumeroLigne { get; set; }
    public string NumeroBon { get; set; } = string.Empty;
    public string NumeroBonLivraison { get; set; } = string.Empty;
    public string CodeProduit { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public decimal Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal Remise { get; set; }
    public decimal MontantHT { get; set; }
    public decimal TauxTVA { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }
    
    // Additional properties for BL to Facture conversion
    public decimal TauxRemise { get; set; }
    public decimal TauxFodec { get; set; }
    public decimal MontantFodec { get; set; }

    // Navigation properties
    public BonLivraison? BonLivraison { get; set; }
    public Produit? Produit { get; set; }
}
