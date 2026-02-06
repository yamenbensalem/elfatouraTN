using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Achats.FacturesFournisseur.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Achats.FacturesFournisseur.Commands.CreateFactureFournisseur;

public class CreateFactureFournisseurCommandHandler : IRequestHandler<CreateFactureFournisseurCommand, FactureFournisseurDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public CreateFactureFournisseurCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<FactureFournisseurDto> Handle(CreateFactureFournisseurCommand request, CancellationToken cancellationToken)
    {
        // Vérifier que le fournisseur existe
        var fournisseur = await _unitOfWork.Fournisseurs.GetByCodeAsync(request.CodeFournisseur, _currentUserService.CodeEntreprise);
        if (fournisseur == null)
        {
            throw new InvalidOperationException($"Fournisseur avec le code '{request.CodeFournisseur}' non trouvé.");
        }

        // Générer le numéro de facture interne
        var annee = request.DateFacture.Year;
        var factures = await _unitOfWork.FacturesFournisseur.GetAllAsync();
        var dernierNumero = factures
            .Where(f => f.NumeroFacture.StartsWith($"FF{annee}"))
            .Select(f => f.NumeroFacture)
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

        var numeroFacture = $"FF{annee}-{sequence:D6}";

        // Créer la facture
        var facture = new FactureFournisseur
        {
            NumeroFacture = numeroFacture,
            DateFacture = request.DateFacture,
            DateEcheance = request.DateEcheance,
            CodeFournisseur = request.CodeFournisseur,
            NumeroBonReception = request.NumeroBonReception,
            NumeroFactureFournisseur = request.NumeroFactureFournisseur,
            TauxRemiseGlobale = request.TauxRemiseGlobale,
            Observation = request.Observation,
            Statut = "En attente",
            MontantRegle = 0,
            LignesFacture = new List<LigneFactureFournisseur>()
        };

        decimal montantHT = 0;
        decimal montantTVA = 0;
        decimal montantFodec = 0;
        decimal montantRemise = 0;

        // Traiter les lignes
        foreach (var ligneDto in request.Lignes)
        {
            // Vérifier que le produit existe
            var produit = await _unitOfWork.Produits.GetByCodeAsync(ligneDto.CodeProduit, _currentUserService.CodeEntreprise);
            if (produit == null)
            {
                throw new InvalidOperationException($"Produit avec le code '{ligneDto.CodeProduit}' non trouvé.");
            }

            // Calculer les montants de la ligne
            var montantBrutHT = ligneDto.Quantite * ligneDto.PrixUnitaireHT;
            var remiseLigne = montantBrutHT * (ligneDto.TauxRemise / 100);
            var montantNetHT = montantBrutHT - remiseLigne;
            var fodecLigne = montantNetHT * (ligneDto.TauxFodec / 100);
            var baseTV = montantNetHT + fodecLigne;
            var tvaLigne = baseTV * (ligneDto.TauxTVA / 100);
            var montantLigneTTC = baseTV + tvaLigne;

            var ligne = new LigneFactureFournisseur
            {
                NumeroFacture = numeroFacture,
                CodeProduit = ligneDto.CodeProduit,
                Quantite = ligneDto.Quantite,
                PrixUnitaireHT = ligneDto.PrixUnitaireHT,
                TauxTVA = ligneDto.TauxTVA,
                TauxRemise = ligneDto.TauxRemise,
                TauxFodec = ligneDto.TauxFodec,
                MontantHT = montantNetHT,
                MontantTVA = tvaLigne,
                MontantFodec = fodecLigne,
                MontantTTC = montantLigneTTC
            };

            facture.LignesFacture.Add(ligne);

            montantHT += montantNetHT;
            montantTVA += tvaLigne;
            montantFodec += fodecLigne;
            montantRemise += remiseLigne;
        }

        // Appliquer la remise globale
        var remiseGlobale = montantHT * (request.TauxRemiseGlobale / 100);
        montantRemise += remiseGlobale;

        facture.MontantHT = montantHT - remiseGlobale;
        facture.MontantTVA = montantTVA;
        facture.MontantFodec = montantFodec;
        facture.MontantRemise = montantRemise;
        facture.MontantTTC = facture.MontantHT + montantTVA + montantFodec;

        // Si c'est un fournisseur soumis à la RAS, calculer la retenue
        if (fournisseur.SoumisRAS)
        {
            var retenue = await _unitOfWork.RetenuesSource.GetByCodeAsync(1);
            if (retenue != null)
            {
                facture.TauxRAS = retenue.Taux;
                facture.MontantRAS = facture.MontantHT * (retenue.Taux / 100);
                facture.NetAPayer = facture.MontantTTC - facture.MontantRAS;
            }
            else
            {
                facture.NetAPayer = facture.MontantTTC;
            }
        }
        else
        {
            facture.NetAPayer = facture.MontantTTC;
        }

        await _unitOfWork.FacturesFournisseur.AddAsync(facture);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<FactureFournisseurDto>(facture);
    }
}
