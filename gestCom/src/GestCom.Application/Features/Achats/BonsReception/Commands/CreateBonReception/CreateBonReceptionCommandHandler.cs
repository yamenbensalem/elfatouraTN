using AutoMapper;
using GestCom.Application.Features.Achats.BonsReception.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Achats.BonsReception.Commands.CreateBonReception;

public class CreateBonReceptionCommandHandler : IRequestHandler<CreateBonReceptionCommand, BonReceptionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateBonReceptionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BonReceptionDto> Handle(CreateBonReceptionCommand request, CancellationToken cancellationToken)
    {
        // Vérifier que le fournisseur existe
        var fournisseur = await _unitOfWork.Fournisseurs.GetByCodeAsync(request.CodeFournisseur, request.CodeEntreprise);
        if (fournisseur == null)
        {
            throw new InvalidOperationException($"Fournisseur avec le code '{request.CodeFournisseur}' non trouvé.");
        }

        // Générer le numéro de bon de réception
        var annee = request.DateReception.Year;
        var bonsReception = await _unitOfWork.BonsReception.GetAllAsync();
        var dernierNumero = bonsReception
            .Where(br => br.NumeroBon.StartsWith($"BR{annee}"))
            .Select(br => br.NumeroBon)
            .OrderByDescending(n => n)
            .FirstOrDefault();

        int sequence = 1;
        if (!string.IsNullOrEmpty(dernierNumero))
        {
            var parts = dernierNumero.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[1], out int lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }

        var numeroBon = $"BR{annee}-{sequence:D6}";

        // Créer le bon de réception
        var bonReception = new BonReception
        {
            NumeroBon = numeroBon,
            CodeEntreprise = request.CodeEntreprise,
            DateReception = request.DateReception,
            CodeFournisseur = request.CodeFournisseur,
            NumeroCommande = request.NumeroCommande,
            Notes = request.Notes,
            Statut = "En cours",
            Lignes = new List<LigneBonReception>()
        };

        decimal montantHT = 0;
        decimal montantTVA = 0;

        // Traiter les lignes
        foreach (var ligneDto in request.Lignes)
        {
            // Vérifier que le produit existe
            var produit = await _unitOfWork.Produits.GetByCodeAsync(ligneDto.CodeProduit, request.CodeEntreprise);
            if (produit == null)
            {
                throw new InvalidOperationException($"Produit avec le code '{ligneDto.CodeProduit}' non trouvé.");
            }

            // Calculer les montants de la ligne
            var montantBrutHT = ligneDto.Quantite * ligneDto.PrixUnitaire;
            var remiseLigne = montantBrutHT * (ligneDto.Remise / 100);
            var montantNetHT = montantBrutHT - remiseLigne;
            var tvaLigne = montantNetHT * (ligneDto.TauxTVA / 100);
            var montantLigneTTC = montantNetHT + tvaLigne;

            var ligne = new LigneBonReception
            {
                NumeroBon = numeroBon,
                CodeProduit = ligneDto.CodeProduit,
                Designation = produit.Designation,
                Quantite = ligneDto.Quantite,
                PrixUnitaire = ligneDto.PrixUnitaire,
                Remise = ligneDto.Remise,
                TauxTVA = ligneDto.TauxTVA,
                MontantHT = montantNetHT,
                MontantTVA = tvaLigne,
                MontantTTC = montantLigneTTC
            };

            bonReception.Lignes.Add(ligne);

            montantHT += montantNetHT;
            montantTVA += tvaLigne;

            // INCRÉMENTER le stock (réception = entrée en stock)
            produit.Quantite += ligneDto.Quantite;
            await _unitOfWork.Produits.UpdateAsync(produit);
        }

        bonReception.MontantHT = montantHT;
        bonReception.MontantTVA = montantTVA;
        bonReception.MontantTTC = montantHT + montantTVA;

        // Si lié à une commande d'achat, mettre à jour le statut
        if (!string.IsNullOrEmpty(request.NumeroCommande))
        {
            var commande = await _unitOfWork.CommandesAchat.GetByNumeroAsync(request.NumeroCommande, request.CodeEntreprise);
            if (commande != null)
            {
                commande.Statut = "Réceptionnée";
                await _unitOfWork.CommandesAchat.UpdateAsync(commande);
            }
        }

        await _unitOfWork.BonsReception.AddAsync(bonReception);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<BonReceptionDto>(bonReception);
    }
}
