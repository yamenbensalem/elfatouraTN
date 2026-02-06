using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Ventes.Factures.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Ventes.BonsLivraison.Commands.ConvertBLToFacture;

public class ConvertBLToFactureCommandHandler : IRequestHandler<ConvertBLToFactureCommand, FactureClientDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public ConvertBLToFactureCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<FactureClientDto> Handle(ConvertBLToFactureCommand request, CancellationToken cancellationToken)
    {
        if (request.NumerosBonLivraison == null || !request.NumerosBonLivraison.Any())
        {
            throw new InvalidOperationException("Au moins un bon de livraison doit être spécifié.");
        }

        // Récupérer les bons de livraison
        var bonsLivraison = new List<BonLivraison>();
        string? codeClient = null;

        foreach (var numeroBL in request.NumerosBonLivraison)
        {
            var bl = await _unitOfWork.BonsLivraison.GetByNumeroAsync(numeroBL, _currentUserService.CodeEntreprise);
            if (bl == null)
            {
                throw new InvalidOperationException($"Bon de livraison '{numeroBL}' non trouvé.");
            }

            // Vérifier que le BL n'est pas déjà facturé
            var factures = await _unitOfWork.FacturesClient.GetAllAsync();
            var factureExistante = factures.FirstOrDefault(f => f.NumeroBonLivraison == numeroBL);
            if (factureExistante != null)
            {
                throw new InvalidOperationException(
                    $"Le bon de livraison '{numeroBL}' est déjà lié à la facture '{factureExistante.NumeroFacture}'.");
            }

            // Vérifier que tous les BL sont du même client
            if (codeClient == null)
            {
                codeClient = bl.CodeClient;
            }
            else if (bl.CodeClient != codeClient)
            {
                throw new InvalidOperationException(
                    "Tous les bons de livraison doivent être du même client.");
            }

            bonsLivraison.Add(bl);
        }

        // Générer le numéro de facture
        var annee = request.DateFacture.Year;
        var allFactures = await _unitOfWork.FacturesClient.GetAllAsync();
        var dernierNumero = allFactures
            .Where(f => f.NumeroFacture.StartsWith($"FC{annee}"))
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

        var numeroFacture = $"FC{annee}-{sequence:D6}";

        // Créer la facture
        var facture = new FactureClient
        {
            NumeroFacture = numeroFacture,
            DateFacture = request.DateFacture,
            DateEcheance = request.DateEcheance,
            CodeClient = codeClient!,
            NumeroBonLivraison = string.Join(", ", request.NumerosBonLivraison),
            TauxRemiseGlobale = request.TauxRemiseGlobale,
            Observation = request.Observation,
            Statut = "En attente",
            MontantRegle = 0,
            LignesFacture = new List<LigneFactureClient>()
        };

        decimal montantHT = 0;
        decimal montantTVA = 0;
        decimal montantFodec = 0;
        decimal montantRemise = 0;

        // Consolider les lignes de tous les BL
        foreach (var bl in bonsLivraison)
        {
            if (bl.LignesBonLivraison != null)
            {
                foreach (var ligneBL in bl.LignesBonLivraison)
                {
                    var ligneFacture = new LigneFactureClient
                    {
                        NumeroFacture = numeroFacture,
                        CodeProduit = ligneBL.CodeProduit,
                        Quantite = ligneBL.Quantite,
                        PrixUnitaireHT = ligneBL.PrixUnitaireHT,
                        TauxTVA = ligneBL.TauxTVA,
                        TauxRemise = ligneBL.TauxRemise,
                        TauxFodec = ligneBL.TauxFodec,
                        MontantHT = ligneBL.MontantHT,
                        MontantTVA = ligneBL.MontantTVA,
                        MontantFodec = ligneBL.MontantFodec,
                        MontantTTC = ligneBL.MontantTTC
                    };

                    facture.LignesFacture.Add(ligneFacture);

                    montantHT += ligneBL.MontantHT;
                    montantTVA += ligneBL.MontantTVA;
                    montantFodec += ligneBL.MontantFodec;
                    montantRemise += ligneBL.MontantHT * (ligneBL.TauxRemise / 100);
                }
            }

            // Marquer le BL comme facturé
            bl.Statut = "Facturé";
            await _unitOfWork.BonsLivraison.UpdateAsync(bl);
        }

        // Appliquer la remise globale
        var remiseGlobale = montantHT * (request.TauxRemiseGlobale / 100);
        montantRemise += remiseGlobale;

        facture.MontantHT = montantHT - remiseGlobale;
        facture.MontantTVA = montantTVA;
        facture.MontantFodec = montantFodec;
        facture.MontantRemise = montantRemise;
        facture.MontantTTC = facture.MontantHT + montantTVA + montantFodec;

        // Gérer la RAS si le client est soumis
        var client = await _unitOfWork.Clients.GetByCodeAsync(codeClient!, _currentUserService.CodeEntreprise);
        if (client?.SoumisRAS == true)
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

        await _unitOfWork.FacturesClient.AddAsync(facture);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<FactureClientDto>(facture);
    }
}
