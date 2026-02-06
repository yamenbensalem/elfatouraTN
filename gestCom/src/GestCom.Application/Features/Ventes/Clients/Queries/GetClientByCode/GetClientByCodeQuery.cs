using GestCom.Application.Features.Ventes.Clients.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.Clients.Queries.GetClientByCode;

/// <summary>
/// Requête pour obtenir un client par son code
/// </summary>
public class GetClientByCodeQuery : IRequest<ClientDto?>
{
    public string CodeClient { get; set; } = string.Empty;
}
