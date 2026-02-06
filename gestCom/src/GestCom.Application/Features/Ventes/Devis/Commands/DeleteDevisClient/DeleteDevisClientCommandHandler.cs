using GestCom.Application.Common.Interfaces;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Ventes.Devis.Commands.DeleteDevisClient;

public class DeleteDevisClientCommandHandler : IRequestHandler<DeleteDevisClientCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteDevisClientCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteDevisClientCommand request, CancellationToken cancellationToken)
    {
        var devis = await _unitOfWork.DevisClients.GetByNumeroAsync(request.NumeroDevis, _currentUserService.CodeEntreprise);
        if (devis == null)
        {
            throw new InvalidOperationException($"Devis '{request.NumeroDevis}' non trouvé.");
        }

        // Vérifier que le devis n'a pas été converti en commande
        if (devis.Statut == "Converti" || !string.IsNullOrEmpty(devis.NumeroCommande))
        {
            throw new InvalidOperationException(
                "Impossible de supprimer un devis converti en commande. Veuillez d'abord supprimer la commande associée.");
        }

        await _unitOfWork.DevisClients.DeleteAsync(devis);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
