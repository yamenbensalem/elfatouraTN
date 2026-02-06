using GestCom.Application.Features.Reporting.DTOs;
using MediatR;

namespace GestCom.Application.Features.Reporting.Queries.GetRapportCreances;

/// <summary>
/// Query pour récupérer le rapport des créances clients
/// </summary>
public class GetRapportCreancesQuery : IRequest<RapportCreancesDto>
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
    /// Afficher uniquement les créances échues
    /// </summary>
    public bool SeulementEchues { get; set; }
}
