using MediatR;

namespace GestCom.Application.Features.Ventes.Clients.Commands.DeleteClient;

/// <summary>
/// Commande pour supprimer un client
/// </summary>
public class DeleteClientCommand : IRequest<Unit>
{
    public string CodeClient { get; set; } = string.Empty;
}
