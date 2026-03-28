using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Ventes.Produits.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Exceptions;
using MediatR;

namespace GestCom.Application.Features.Ventes.Produits.Commands.CreateProduit;

public class CreateProduitCommandHandler : IRequestHandler<CreateProduitCommand, ProduitDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public CreateProduitCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<ProduitDto> Handle(CreateProduitCommand request, CancellationToken cancellationToken)
    {
        var codeEntreprise = _currentUserService.CodeEntreprise
            ?? throw new InvalidOperationException("Code entreprise introuvable pour l'utilisateur courant.");

        // Vérifier si le produit existe déjà
        var existingProduit = await _unitOfWork.Produits.GetByCodeAsync(request.CodeProduit, codeEntreprise);
        if (existingProduit != null)
        {
            throw new BusinessException($"Un produit avec le code '{request.CodeProduit}' existe déjà.");
        }

        // Vérifier si le code barre est unique
        if (!string.IsNullOrEmpty(request.CodeBarre))
        {
            var produitByCodeBarre = await _unitOfWork.Produits.GetByCodeBarreAsync(request.CodeBarre, codeEntreprise);
            if (produitByCodeBarre != null)
            {
                throw new BusinessException($"Un produit avec le code barre '{request.CodeBarre}' existe déjà.");
            }
        }

        // Vérifier si le fournisseur existe
        if (!string.IsNullOrEmpty(request.CodeFournisseur))
        {
            var fournisseur = await _unitOfWork.Fournisseurs.GetByCodeAsync(request.CodeFournisseur, codeEntreprise);
            if (fournisseur == null)
            {
                throw new NotFoundException("Fournisseur", request.CodeFournisseur);
            }
        }

        // Créer l'entité
        var produit = _mapper.Map<Produit>(request);
        produit.CodeEntreprise = codeEntreprise;
        produit.DateCreation = DateTime.Now;
        if (produit.CodeDevise == 0) produit.CodeDevise = 5; // Défaut TND

        // Calculer les prix si nécessaire
        if (request.TauxMarge > 0 && request.PrixAchatTTC > 0)
        {
            produit.PrixVenteHT = request.PrixAchatTTC * (1 + request.TauxMarge / 100);
            produit.PrixVenteTTC = produit.PrixVenteHT * (1 + request.TauxTVA / 100);
        }

        await _unitOfWork.Produits.AddAsync(produit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Recharger avec les relations
        var createdProduit = await _unitOfWork.Produits.GetByCodeAsync(produit.CodeProduit, codeEntreprise);
        return _mapper.Map<ProduitDto>(createdProduit);
    }
}
