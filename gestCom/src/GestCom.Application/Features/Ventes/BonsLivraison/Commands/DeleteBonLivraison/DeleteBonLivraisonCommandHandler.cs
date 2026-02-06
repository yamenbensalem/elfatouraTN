using GestCom.Application.Common.Interfaces;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Ventes.BonsLivraison.Commands.DeleteBonLivraison;

public class DeleteBonLivraisonCommandHandler : IRequestHandler<DeleteBonLivraisonCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteBonLivraisonCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteBonLivraisonCommand request, CancellationToken cancellationToken)
    {
        var bonLivraison = await _unitOfWork.BonsLivraison.GetByNumeroAsync(request.NumeroBonLivraison, _currentUserService.CodeEntreprise);
        if (bonLivraison == null)
        {
            throw new InvalidOperationException($"Bon de livraison '{request.NumeroBonLivraison}' non trouvé.");
        }

        // Vérifier qu'il n'y a pas de facture liée
        var factures = await _unitOfWork.FacturesClient.GetAllAsync();
        var factureAssociee = factures.FirstOrDefault(f => f.NumeroBonLivraison == request.NumeroBonLivraison);
        if (factureAssociee != null)
        {
            throw new InvalidOperationException(
                $"Impossible de supprimer ce bon de livraison car il est lié à la facture '{factureAssociee.NumeroFacture}'.");
        }

        // Restaurer le stock si demandé
        if (request.RestaurerStock && bonLivraison.LignesBonLivraison != null)
        {
            foreach (var ligne in bonLivraison.LignesBonLivraison)
            {
                var produit = await _unitOfWork.Produits.GetByCodeAsync(ligne.CodeProduit, _currentUserService.CodeEntreprise);
                if (produit != null)
                {
                    produit.Quantite += ligne.Quantite; // Réincrémenter le stock
                    await _unitOfWork.Produits.UpdateAsync(produit);
                }
            }
        }

        // Mettre à jour le statut de la commande si liée
        if (!string.IsNullOrEmpty(bonLivraison.NumeroCommande))
        {
            var commande = await _unitOfWork.CommandesVente.GetByNumeroAsync(bonLivraison.NumeroCommande, _currentUserService.CodeEntreprise);
            if (commande != null)
            {
                commande.Statut = "En attente";
                await _unitOfWork.CommandesVente.UpdateAsync(commande);
            }
        }

        await _unitOfWork.BonsLivraison.DeleteAsync(bonLivraison);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
