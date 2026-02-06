using MediatR;
using GestCom.Application.Features.Ventes.Clients.DTOs;

namespace GestCom.Application.Features.Ventes.Clients.Queries.GetClientById;

/// <summary>
/// Requête pour obtenir un client par son code
/// </summary>
public class GetClientByIdQuery : IRequest<ClientDto?>
{
    public string CodeClient { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
}
