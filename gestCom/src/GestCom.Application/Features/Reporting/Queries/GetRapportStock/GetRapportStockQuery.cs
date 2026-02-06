using GestCom.Application.Features.Reporting.DTOs;
using MediatR;

namespace GestCom.Application.Features.Reporting.Queries.GetRapportStock;

/// <summary>
/// Query pour récupérer le rapport de stock
/// </summary>
public class GetRapportStockQuery : IRequest<RapportStockDto>
{
    /// <summary>
    /// Filtrer par catégorie
    /// </summary>
    public string? CodeCategorie { get; set; }
    
    /// <summary>
    /// Filtrer par magasin
    /// </summary>
    public string? CodeMagasin { get; set; }
    
    /// <summary>
    /// Seuil de stock faible (pourcentage du stock minimal)
    /// </summary>
    public decimal SeuilStockFaible { get; set; } = 1.5m; // 150% du stock minimal
}
