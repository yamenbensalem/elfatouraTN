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
        var codeEntreprise = _currentUserService.CodeEntreprise
            ?? throw new InvalidOperationException("Code entreprise introuvable pour l'utilisateur courant.");

        var produit = await _unitOfWork.Produits.GetByCodeAsync(request.CodeProduit, codeEntreprise);
        if (produit == null)
        {
            throw new NotFoundException("Produit", request.CodeProduit);
        }

        // Vérifier si le produit n'est pas utilisé dans des documents
        var hasLignesFacture = await _unitOfWork.Produits.HasLignesFactureAsync(request.CodeProduit, codeEntreprise);
        if (hasLignesFacture)
        {
            throw new BusinessException($"Impossible de supprimer le produit '{request.CodeProduit}' car il est utilisé dans des factures.");
        }

        var hasLignesCommande = await _unitOfWork.Produits.HasLignesCommandeAsync(request.CodeProduit, codeEntreprise);
        if (hasLignesCommande)
        {
            throw new BusinessException($"Impossible de supprimer le produit '{request.CodeProduit}' car il est utilisé dans des commandes.");
        }

        _unitOfWork.Produits.Delete(produit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
