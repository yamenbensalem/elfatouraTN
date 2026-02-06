using MediatR;
using GestCom.Application.Features.Ventes.Clients.DTOs;
using GestCom.Shared.Common;

namespace GestCom.Application.Features.Ventes.Clients.Queries.GetAllClients;

/// <summary>
/// Requête pour obtenir tous les clients avec pagination
/// </summary>
public class GetAllClientsQuery : IRequest<PagedResult<ClientListDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public string? Etat { get; set; } // Actif, Inactif, Bloqué
    public bool? Etranger { get; set; }
}
