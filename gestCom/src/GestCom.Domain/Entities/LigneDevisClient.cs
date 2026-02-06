using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Ligne de devis client
/// </summary>
public class LigneDevisClient : BaseEntity
{
    public int Id { get; set; }
    public int NumeroLigne { get; set; }
    public string NumeroDevis { get; set; } = string.Empty;
    public string CodeProduit { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public decimal Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal Remise { get; set; }
    public decimal TauxRemise { get; set; }
    public decimal MontantRemise { get; set; }
    public decimal MontantHT { get; set; }
    public decimal TauxTVA { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal TauxFodec { get; set; }
    public decimal MontantFodec { get; set; }
    public decimal MontantTTC { get; set; }

    // Navigation properties
    public DevisClient? DevisClient { get; set; }
    public Produit? Produit { get; set; }
}
