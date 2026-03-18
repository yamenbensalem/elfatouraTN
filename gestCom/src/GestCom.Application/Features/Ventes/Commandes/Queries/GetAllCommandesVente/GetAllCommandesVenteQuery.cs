using GestCom.Application.Features.Ventes.Commandes.DTOs;
using GestCom.Shared.Common;
using MediatR;

namespace GestCom.Application.Features.Ventes.Commandes.Queries.GetAllCommandesVente;

public class GetAllCommandesVenteQuery : IRequest<PagedResult<CommandeVenteListDto>>
{
    public string? CodeClient { get; set; }
    public string? Statut { get; set; }
    public DateTime? DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
