using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Ligne de bon de réception
/// </summary>
public class LigneBonReception : BaseEntity
{
    public int Id { get; set; }
    public int NumeroLigne { get; set; }
    public string NumeroBon { get; set; } = string.Empty;
    public string CodeProduit { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public decimal Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal Remise { get; set; }
    public decimal MontantHT { get; set; }
    public decimal TauxTVA { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }

    // Navigation properties
    public BonReception? BonReception { get; set; }
    public Produit? Produit { get; set; }
}
