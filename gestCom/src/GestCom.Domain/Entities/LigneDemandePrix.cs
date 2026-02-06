using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Ligne de demande de prix
/// </summary>
public class LigneDemandePrix : BaseEntity
{
    public int Id { get; set; }
    public string NumeroDemande { get; set; } = string.Empty;
    public string CodeProduit { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public decimal Quantite { get; set; }
    public decimal? PrixPropose { get; set; }

    // Navigation properties
    public DemandePrix? DemandePrix { get; set; }
    public Produit? Produit { get; set; }
}
