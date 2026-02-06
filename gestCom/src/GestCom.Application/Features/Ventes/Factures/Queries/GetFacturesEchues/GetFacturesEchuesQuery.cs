using GestCom.Application.Features.Ventes.Factures.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.Factures.Queries.GetFacturesEchues;

/// <summary>
/// Query pour récupérer les factures échues
/// </summary>
public class GetFacturesEchuesQuery : IRequest<IEnumerable<FactureEchueDto>>
{
    /// <summary>
    /// Date de référence pour calculer les retards
    /// </summary>
    public DateTime? DateReference { get; set; }
    
    /// <summary>
    /// Filtrer par client
    /// </summary>
    public string? CodeClient { get; set; }
    
    /// <summary>
    /// Nombre minimum de jours de retard
    /// </summary>
    public int? JoursRetardMinimum { get; set; }
}

public class FactureEchueDto
{
    public string NumeroFacture { get; set; } = string.Empty;
    public DateTime DateFacture { get; set; }
    public DateTime DateEcheance { get; set; }
    public string CodeClient { get; set; } = string.Empty;
    public string? NomClient { get; set; }
    public string? Telephone { get; set; }
    public string? Email { get; set; }
    public decimal MontantTTC { get; set; }
    public decimal MontantRegle { get; set; }
    public decimal ResteAPayer { get; set; }
    public int JoursRetard { get; set; }
    public string CategorieRetard { get; set; } = string.Empty; // "1-30 jours", "31-60 jours", etc.
}
