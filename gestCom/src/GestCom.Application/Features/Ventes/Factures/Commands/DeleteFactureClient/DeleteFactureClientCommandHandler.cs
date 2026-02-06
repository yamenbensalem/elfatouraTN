using GestCom.Application.Common.Interfaces;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Ventes.Factures.Commands.DeleteFactureClient;

public class DeleteFactureClientCommandHandler : IRequestHandler<DeleteFactureClientCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteFactureClientCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteFactureClientCommand request, CancellationToken cancellationToken)
    {
        var facture = await _unitOfWork.FacturesClient.GetByNumeroAsync(request.NumeroFacture, _currentUserService.CodeEntreprise);
        if (facture == null)
        {
            throw new InvalidOperationException($"Facture '{request.NumeroFacture}' non trouvée.");
        }

        // Vérifier qu'il n'y a pas de règlements
        if (facture.MontantRegle > 0)
        {
            throw new InvalidOperationException(
                "Impossible de supprimer une facture ayant des règlements. Veuillez d'abord annuler les règlements.");
        }

        // Restaurer le stock si demandé
        if (request.RestaurerStock && facture.LignesFacture != null)
        {
            foreach (var ligne in facture.LignesFacture)
            {
                var produit = await _unitOfWork.Produits.GetByCodeAsync(ligne.CodeProduit, _currentUserService.CodeEntreprise);
                if (produit != null)
                {
                    produit.Quantite += ligne.Quantite; // Réincrémenter le stock
                    await _unitOfWork.Produits.UpdateAsync(produit);
                }
            }
        }

        await _unitOfWork.FacturesClient.DeleteAsync(facture);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
