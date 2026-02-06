using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Ventes.Produits.DTOs;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Exceptions;
using MediatR;

namespace GestCom.Application.Features.Ventes.Produits.Commands.UpdateProduit;

public class UpdateProduitCommandHandler : IRequestHandler<UpdateProduitCommand, ProduitDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public UpdateProduitCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<ProduitDto> Handle(UpdateProduitCommand request, CancellationToken cancellationToken)
    {
        var produit = await _unitOfWork.Produits.GetByCodeAsync(request.CodeProduit, _currentUserService.CodeEntreprise);
        if (produit == null)
        {
            throw new NotFoundException("Produit", request.CodeProduit);
        }

        // Vérifier unicité du code barre
        if (!string.IsNullOrEmpty(request.CodeBarre) && request.CodeBarre != produit.CodeBarre)
        {
            var existingByCodeBarre = await _unitOfWork.Produits.GetByCodeBarreAsync(request.CodeBarre, _currentUserService.CodeEntreprise);
            if (existingByCodeBarre != null && existingByCodeBarre.CodeProduit != request.CodeProduit)
            {
                throw new BusinessException($"Un produit avec le code barre '{request.CodeBarre}' existe déjà.");
            }
        }

        // Mettre à jour les propriétés
        produit.Designation = request.Designation;
        produit.CodeBarre = request.CodeBarre;
        produit.Reference = request.Reference;
        produit.PrixAchatTTC = request.PrixAchatTTC;
        produit.TauxMarge = request.TauxMarge;
        produit.PrixVenteHT = request.PrixVenteHT;
        produit.PrixVenteTTC = request.PrixVenteTTC;
        produit.Quantite = request.Quantite;
        produit.StockMinimal = request.StockMinimal;
        produit.TauxTVA = request.TauxTVA;
        produit.TauxFODEC = request.TauxFODEC;
        produit.Fodec = request.Fodec ? 1m : 0m;
        produit.CodeFournisseur = request.CodeFournisseur;
        produit.CodeUnite = request.CodeUnite;
        produit.CodeCategorie = request.CodeCategorie;
        produit.CodeMagasin = request.CodeMagasin;
        if (int.TryParse(request.CodeTVA, out var codeTVA))
        {
            produit.CodeTVA = codeTVA;
        }

        // Recalculer les prix si marge changée
        if (request.TauxMarge > 0 && request.PrixAchatTTC > 0)
        {
            produit.PrixVenteHT = request.PrixAchatTTC * (1 + request.TauxMarge / 100);
            produit.PrixVenteTTC = produit.PrixVenteHT * (1 + request.TauxTVA / 100);
        }

        _unitOfWork.Produits.Update(produit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedProduit = await _unitOfWork.Produits.GetByCodeAsync(produit.CodeProduit, _currentUserService.CodeEntreprise);
        return _mapper.Map<ProduitDto>(updatedProduit);
    }
}
