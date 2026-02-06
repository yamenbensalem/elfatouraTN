using GestCom.Application.Features.Ventes.Produits.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.Produits.Queries.GetProduitsStockFaible;

/// <summary>
/// Query pour récupérer les produits avec stock faible
/// </summary>
public class GetProduitsStockFaibleQuery : IRequest<IEnumerable<ProduitStockAlertDto>>
{
    /// <summary>
    /// Inclure les produits en rupture de stock
    /// </summary>
    public bool InclureRupture { get; set; } = true;
    
    /// <summary>
    /// Filtrer par catégorie
    /// </summary>
    public string? CodeCategorie { get; set; }
}

public class ProduitStockAlertDto
{
    public string CodeProduit { get; set; } = string.Empty;
    public string? CodeBarre { get; set; }
    public string? Designation { get; set; }
    public string? LibelleCategorie { get; set; }
    public string? NomFournisseur { get; set; }
    public decimal Quantite { get; set; }
    public decimal StockMinimal { get; set; }
    public decimal QuantiteACommander { get; set; }
    public string Niveau { get; set; } = string.Empty; // "Rupture", "Critique", "Faible"
    public decimal PrixAchatTTC { get; set; }
}
