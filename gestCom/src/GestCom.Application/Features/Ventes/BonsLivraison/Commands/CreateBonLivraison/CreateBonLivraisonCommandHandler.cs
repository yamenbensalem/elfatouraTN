using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Ventes.BonsLivraison.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Exceptions;
using MediatR;

namespace GestCom.Application.Features.Ventes.BonsLivraison.Commands.CreateBonLivraison;

public class CreateBonLivraisonCommandHandler : IRequestHandler<CreateBonLivraisonCommand, BonLivraisonDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public CreateBonLivraisonCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<BonLivraisonDto> Handle(CreateBonLivraisonCommand request, CancellationToken cancellationToken)
    {
        var client = await _unitOfWork.Clients.GetByCodeAsync(request.CodeClient, _currentUserService.CodeEntreprise);
        if (client == null)
        {
            throw new NotFoundException("Client", request.CodeClient);
        }

        var numeroBL = await GenerateNumeroBonLivraisonAsync();

        var bonLivraison = new BonLivraison
        {
            CodeEntreprise = _currentUserService.CodeEntreprise!,
            NumeroBonLivraison = numeroBL,
            DateBonLivraison = request.DateBonLivraison,
            CodeClient = request.CodeClient,
            AdresseLivraison = request.AdresseLivraison ?? client.Adresse,
            NumeroCommande = request.NumeroCommande,
            Observations = request.Observations,
            Statut = "Livré"
        };

        int numeroLigne = 1;
        decimal totalHT = 0;
        decimal totalTVA = 0;

        foreach (var ligneDto in request.Lignes)
        {
            var produit = await _unitOfWork.Produits.GetByCodeAsync(ligneDto.CodeProduit, _currentUserService.CodeEntreprise);
            if (produit == null)
            {
                throw new NotFoundException("Produit", ligneDto.CodeProduit);
            }

            if (produit.Quantite < ligneDto.Quantite)
            {
                throw new BusinessException($"Stock insuffisant pour le produit '{produit.Designation}'. Disponible: {produit.Quantite}, Demandé: {ligneDto.Quantite}");
            }

            var montantHT = ligneDto.Quantite * ligneDto.PrixUnitaireHT;
            var montantTVA = montantHT * (ligneDto.TauxTVA / 100);
            var montantTTC = montantHT + montantTVA;

            var ligne = new LigneBonLivraison
            {
                NumeroBonLivraison = numeroBL,
                NumeroLigne = numeroLigne++,
                CodeProduit = ligneDto.CodeProduit,
                Quantite = ligneDto.Quantite,
                PrixUnitaireHT = ligneDto.PrixUnitaireHT,
                TauxTVA = ligneDto.TauxTVA,
                MontantHT = montantHT,
                MontantTVA = montantTVA,
                MontantTTC = montantTTC
            };

            bonLivraison.LignesBonLivraison.Add(ligne);

            totalHT += montantHT;
            totalTVA += montantTVA;

            // Décrémenter le stock
            produit.Quantite -= ligneDto.Quantite;
            _unitOfWork.Produits.Update(produit);
        }

        bonLivraison.MontantHT = totalHT;
        bonLivraison.MontantTVA = totalTVA;
        bonLivraison.MontantTTC = totalHT + totalTVA;

        await _unitOfWork.BonsLivraison.AddAsync(bonLivraison);

        // Mettre à jour la commande si référencée
        if (!string.IsNullOrEmpty(request.NumeroCommande))
        {
            var commande = await _unitOfWork.CommandesVente.GetByNumeroAsync(request.NumeroCommande, _currentUserService.CodeEntreprise);
            if (commande != null)
            {
                commande.NumeroBonLivraison = numeroBL;
                commande.Statut = "Livré";
                _unitOfWork.CommandesVente.Update(commande);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdBL = await _unitOfWork.BonsLivraison.GetByNumeroAsync(numeroBL, _currentUserService.CodeEntreprise);
        return _mapper.Map<BonLivraisonDto>(createdBL);
    }

    private async Task<string> GenerateNumeroBonLivraisonAsync()
    {
        var year = DateTime.Now.Year;
        var lastNumber = await _unitOfWork.BonsLivraison.GetLastNumeroAsync(_currentUserService.CodeEntreprise);
        
        int nextNumber = 1;
        if (!string.IsNullOrEmpty(lastNumber))
        {
            var parts = lastNumber.Split('-');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int last))
            {
                nextNumber = last + 1;
            }
        }

        return $"BL{year}-{nextNumber:D6}";
    }
}
