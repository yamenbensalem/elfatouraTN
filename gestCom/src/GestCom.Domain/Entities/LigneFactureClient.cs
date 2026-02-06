using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Ligne de facture client
/// </summary>
public class LigneFactureClient : BaseEntity
{
    public int Id { get; set; }
    public int NumeroLigne { get; set; }
    public string NumeroFacture { get; set; } = string.Empty;
    public string CodeProduit { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public decimal Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal Remise { get; set; }
    public decimal TauxRemise { get; set; }
    public decimal MontantRemise { get; set; }
    public decimal TauxFodec { get; set; }
    public decimal TauxFODEC { get => TauxFodec; set => TauxFodec = value; } // Alias
    public decimal MontantFodec { get; set; }
    public decimal MontantFODEC { get => MontantFodec; set => MontantFodec = value; } // Alias
    public decimal MontantHT { get; set; }
    public decimal TauxTVA { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }

    // Navigation properties
    public FactureClient? FactureClient { get; set; }
    public Produit? Produit { get; set; }
}
