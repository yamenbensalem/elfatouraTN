using GestCom.Application.Common.Interfaces;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Exceptions;
using MediatR;

namespace GestCom.Application.Features.Ventes.Clients.Commands.DeleteClient;

/// <summary>
/// Handler pour la suppression d'un client
/// </summary>
public class DeleteClientCommandHandler : IRequestHandler<DeleteClientCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteClientCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(DeleteClientCommand request, CancellationToken cancellationToken)
    {
        // Récupérer le client
        var client = await _unitOfWork.Clients.GetByCodeAsync(request.CodeClient, _currentUserService.CodeEntreprise);
        if (client == null)
        {
            throw new NotFoundException($"Client avec le code '{request.CodeClient}' non trouvé.");
        }

        // Vérifier si le client a des transactions
        if (client.NombreTransactions > 0)
        {
            throw new BusinessException($"Impossible de supprimer le client '{request.CodeClient}' car il possède {client.NombreTransactions} transaction(s).");
        }

        // Supprimer le client
        await _unitOfWork.Clients.DeleteAsync(client);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
