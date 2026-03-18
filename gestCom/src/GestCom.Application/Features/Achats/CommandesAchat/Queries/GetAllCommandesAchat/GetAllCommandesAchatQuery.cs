using GestCom.Application.Features.Achats.CommandesAchat.DTOs;
using GestCom.Shared.Common;
using MediatR;

namespace GestCom.Application.Features.Achats.CommandesAchat.Queries.GetAllCommandesAchat;

public class GetAllCommandesAchatQuery : IRequest<PagedResult<CommandeAchatListDto>>
{
    public string? CodeFournisseur { get; set; }
    public string? Statut { get; set; }
    public DateTime? DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
