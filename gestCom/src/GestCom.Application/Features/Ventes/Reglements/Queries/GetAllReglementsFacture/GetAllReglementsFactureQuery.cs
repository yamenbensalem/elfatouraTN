using GestCom.Application.Features.Ventes.Reglements.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.Reglements.Queries.GetAllReglementsFacture;

public class GetAllReglementsFactureQuery : IRequest<IEnumerable<ReglementFactureListDto>>
{
    public string? CodeClient { get; set; }
    public string? NumeroFacture { get; set; }
    public DateTime? DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
}