using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Achats.CommandesAchat.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Exceptions;
using MediatR;
using System.Linq;

namespace GestCom.Application.Features.Achats.CommandesAchat.Commands.UpdateCommandeAchat;

public class UpdateCommandeAchatCommandHandler : IRequestHandler<UpdateCommandeAchatCommand, CommandeAchatDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCommandeAchatCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<CommandeAchatDto> Handle(UpdateCommandeAchatCommand request, CancellationToken cancellationToken)
    {
        var codeEntreprise = _currentUserService.CodeEntreprise;

        var commande = await _unitOfWork.CommandesAchat.GetByNumeroAsync(request.NumeroCommande, codeEntreprise);
        if (commande == null)
        {
            throw new NotFoundException("CommandeAchat", request.NumeroCommande);
        }

        if (commande.Statut != "Brouillon" && commande.Statut != "En cours")
        {
            throw new BusinessException($"Impossible de modifier une commande avec le statut {commande.Statut}");
        }

        var fournisseur = await _unitOfWork.Fournisseurs.GetByCodeAsync(request.CodeFournisseur, codeEntreprise);
        if (fournisseur == null)
        {
            throw new NotFoundException("Fournisseur", request.CodeFournisseur);
        }

        commande.DateCommande = request.DateCommande;
        commande.DateLivraison = request.DateLivraisonPrevue;
        commande.CodeFournisseur = request.CodeFournisseur;
        commande.Remise = request.Remise;
        commande.Notes = request.Observation;

        if (!string.IsNullOrEmpty(request.Statut)) {
            commande.Statut = request.Statut;
        }

        commande.Lignes.Clear();

        decimal totalHT = 0;
        decimal totalTVA = 0;

        foreach (var ligneDto in request.Lignes)
        {
            var produit = await _unitOfWork.Produits.GetByCodeAsync(ligneDto.CodeProduit, codeEntreprise);
            if (produit == null)
            {
                throw new NotFoundException("Produit", ligneDto.CodeProduit);
            }

            var montantBrut = ligneDto.Quantite * ligneDto.PrixUnitaireHT;
            var remiseLigne = montantBrut * (ligneDto.TauxRemise / 100);
            var montantHT = montantBrut - remiseLigne;
            var montantTVA = montantHT * (ligneDto.TauxTVA / 100);
            var montantTTC = montantHT + montantTVA;

            var ligne = new LigneCommandeAchat
            {
                NumeroCommande = request.NumeroCommande,
                CodeProduit = ligneDto.CodeProduit,
                Designation = produit.Designation,
                Quantite = ligneDto.Quantite,
                PrixUnitaire = ligneDto.PrixUnitaireHT,
                Remise = ligneDto.TauxRemise,
                TauxTVA = ligneDto.TauxTVA,
                MontantHT = montantHT,
                MontantTVA = montantTVA,
                MontantTTC = montantTTC
            };

            commande.Lignes.Add(ligne);

            totalHT += montantHT;
            totalTVA += montantTVA;
        }

        // Apply global discount
        var remiseGlobale = totalHT * (request.Remise / 100);
        totalHT -= remiseGlobale;

        commande.MontantHT = totalHT;
        commande.MontantTVA = totalTVA;
        commande.Remise = remiseGlobale;
        commande.MontantTTC = totalHT + totalTVA;

        await _unitOfWork.CommandesAchat.UpdateAsync(commande);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedCommande = await _unitOfWork.CommandesAchat.GetByNumeroAsync(request.NumeroCommande, codeEntreprise);
        return _mapper.Map<CommandeAchatDto>(updatedCommande);
    }
}