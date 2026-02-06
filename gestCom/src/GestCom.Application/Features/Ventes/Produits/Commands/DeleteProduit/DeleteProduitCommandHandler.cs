using GestCom.Application.Common.Interfaces;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Exceptions;
using MediatR;

namespace GestCom.Application.Features.Ventes.Produits.Commands.DeleteProduit;

public class DeleteProduitCommandHandler : IRequestHandler<DeleteProduitCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteProduitCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteProduitCommand request, CancellationToken cancellationToken)
    {
        var produit = await _unitOfWork.Produits.GetByCodeAsync(request.CodeProduit, _currentUserService.CodeEntreprise);
        if (produit == null)
        {
            throw new NotFoundException("Produit", request.CodeProduit);
        }

        // Vérifier si le produit n'est pas utilisé dans des documents
        var hasLignesFacture = await _unitOfWork.Produits.HasLignesFactureAsync(request.CodeProduit, _currentUserService.CodeEntreprise);
        if (hasLignesFacture)
        {
            throw new BusinessException($"Impossible de supprimer le produit '{request.CodeProduit}' car il est utilisé dans des factures.");
        }

        var hasLignesCommande = await _unitOfWork.Produits.HasLignesCommandeAsync(request.CodeProduit, _currentUserService.CodeEntreprise);
        if (hasLignesCommande)
        {
            throw new BusinessException($"Impossible de supprimer le produit '{request.CodeProduit}' car il est utilisé dans des commandes.");
        }

        _unitOfWork.Produits.Delete(produit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
