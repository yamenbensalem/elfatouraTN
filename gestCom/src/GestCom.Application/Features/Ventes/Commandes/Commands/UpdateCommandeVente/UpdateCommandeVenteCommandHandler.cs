using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Ventes.Commandes.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Exceptions;
using MediatR;
using System.Linq;

namespace GestCom.Application.Features.Ventes.Commandes.Commands.UpdateCommandeVente;

public class UpdateCommandeVenteCommandHandler : IRequestHandler<UpdateCommandeVenteCommand, CommandeVenteDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCommandeVenteCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<CommandeVenteDto> Handle(UpdateCommandeVenteCommand request, CancellationToken cancellationToken)
    {
        var codeEntreprise = _currentUserService.CodeEntreprise
            ?? throw new InvalidOperationException("Code entreprise introuvable pour l'utilisateur courant.");

        var commande = await _unitOfWork.CommandesVente.GetByNumeroAsync(request.NumeroCommande, codeEntreprise);
        if (commande == null)
        {
            throw new NotFoundException("CommandeVente", request.NumeroCommande);
        }

        if (commande.Statut != "Brouillon" && commande.Statut != "En cours")
        {
            throw new BusinessException($"Impossible de modifier une commande avec le statut {commande.Statut}");
        }

        // Verify client exists 
        var client = await _unitOfWork.Clients.GetByCodeAsync(request.CodeClient, codeEntreprise);
        if (client == null)
        {
            throw new NotFoundException("Client", request.CodeClient);
        }

        // Update main properties
        commande.DateCommande = request.DateCommande;
        commande.DateLivraisonPrevue = request.DateLivraisonPrevue;
        commande.CodeClient = request.CodeClient;
        commande.AdresseLivraison = request.AdresseLivraison ?? client.Adresse;
        commande.TauxRemise = request.TauxRemise;
        commande.CodeDevise = request.CodeDevise;
        commande.TauxChange = request.TauxChange;
        commande.Observations = request.Observations;
        
        if (!string.IsNullOrEmpty(request.Statut)) {
            commande.Statut = request.Statut;
        }

        // Update Lignes
        // In a real scenario, this would reconcile additions, updates, deletions.
        // For simplicity, recreate the lines.
        commande.Lignes.Clear();

        int numeroLigne = 1;
        decimal totalHT = 0;
        decimal totalTVA = 0;

        foreach (var ligneDto in request.Lignes)
        {
            var produit = await _unitOfWork.Produits.GetByCodeAsync(ligneDto.CodeProduit, codeEntreprise);
            if (produit == null)
            {
                throw new NotFoundException("Produit", ligneDto.CodeProduit);
            }

            var montantBrutHT = ligneDto.Quantite * ligneDto.PrixUnitaireHT;
            var montantRemise = montantBrutHT * (ligneDto.TauxRemise / 100);
            var montantNetHT = montantBrutHT - montantRemise;
            var montantTVA = montantNetHT * (ligneDto.TauxTVA / 100);
            var montantTTC = montantNetHT + montantTVA;

            var ligne = new LigneCommandeVente
            {
                NumeroCommande = request.NumeroCommande,
                NumeroLigne = numeroLigne++,
                CodeProduit = ligneDto.CodeProduit,
                Quantite = ligneDto.Quantite,
                QuantiteLivree = 0,
                PrixUnitaireHT = ligneDto.PrixUnitaireHT,
                TauxTVA = ligneDto.TauxTVA,
                TauxRemise = ligneDto.TauxRemise,
                MontantRemise = montantRemise,
                MontantHT = montantNetHT,
                MontantTVA = montantTVA,
                MontantTTC = montantTTC
            };

            commande.Lignes.Add(ligne);

            totalHT += montantNetHT;
            totalTVA += montantTVA;
        }

        var remiseGlobale = totalHT * (request.TauxRemise / 100);
        totalHT -= remiseGlobale;

        commande.MontantHT = totalHT;
        commande.MontantTVA = totalTVA;
        commande.Remise = remiseGlobale;
        commande.MontantTTC = totalHT + totalTVA;

        await _unitOfWork.CommandesVente.UpdateAsync(commande);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedCommande = await _unitOfWork.CommandesVente.GetByNumeroAsync(request.NumeroCommande, codeEntreprise);
        return _mapper.Map<CommandeVenteDto>(updatedCommande);
    }
}