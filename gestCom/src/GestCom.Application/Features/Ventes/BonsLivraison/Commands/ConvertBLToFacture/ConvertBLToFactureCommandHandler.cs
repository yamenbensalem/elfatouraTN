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
    private readonly INumeroService _numeroService;

    public ConvertBLToFactureCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService,
        INumeroService numeroService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _numeroService = numeroService;
    }

    public async Task<FactureClientDto> Handle(ConvertBLToFactureCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var codeEntreprise = _currentUserService.CodeEntreprise
                ?? throw new InvalidOperationException("Code entreprise introuvable pour l'utilisateur courant.");

            if (request.NumerosBonLivraison == null || !request.NumerosBonLivraison.Any())
            {
                throw new InvalidOperationException("Au moins un bon de livraison doit être spécifié.");
            }

            // Récupérer les bons de livraison
            var bonsLivraison = new List<BonLivraison>();
            string? codeClient = null;
            var facturesExistantes = await _unitOfWork.FacturesClient.GetAllAsync();

            foreach (var numeroBL in request.NumerosBonLivraison)
            {
                var bl = await _unitOfWork.BonsLivraison.GetByNumeroAsync(numeroBL, codeEntreprise);
                if (bl == null)
                {
                    throw new InvalidOperationException($"Bon de livraison '{numeroBL}' non trouvé.");
                }

                // Vérifier que le BL n'est pas déjà facturé
                if (bl.Facture || !string.IsNullOrWhiteSpace(bl.NumeroFacture))
                {
                    throw new InvalidOperationException(
                        $"Le bon de livraison '{numeroBL}' est déjà marqué comme facturé.");
                }

                var factureExistante = facturesExistantes.FirstOrDefault(f =>
                    NumeroBonLivraisonContains(f.NumeroBonLivraison, numeroBL));
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
            var numeroFacture = await _numeroService.GenererNumeroFactureClientAsync(codeEntreprise);

            // Créer la facture
            var facture = new FactureClient
            {
                CodeEntreprise = codeEntreprise,
                NumeroFacture = numeroFacture,
                DateFacture = request.DateFacture,
                DateEcheance = request.DateEcheance == default
                    ? request.DateFacture.AddDays(30)
                    : request.DateEcheance,
                CodeClient = codeClient!,
                NumeroBonLivraison = string.Join(", ", request.NumerosBonLivraison),
                TauxRemiseGlobale = request.TauxRemiseGlobale,
                Observation = request.Observation,
                Origine = "BonLivraison",
                Statut = "En attente",
                MontantRegle = 0,
                Lignes = new List<LigneFactureClient>()
            };

            decimal montantHT = 0;
            decimal montantTVA = 0;
            decimal montantFodec = 0;
            decimal montantRemise = 0;
            var numeroLigne = 1;

            // Consolider les lignes de tous les BL
            foreach (var bl in bonsLivraison)
            {
                if (bl.Lignes != null)
                {
                    foreach (var ligneBL in bl.Lignes)
                    {
                        var montantRemiseLigne = ligneBL.MontantHT * (ligneBL.TauxRemise / 100);
                        var ligneFacture = new LigneFactureClient
                        {
                            NumeroFacture = numeroFacture,
                            NumeroLigne = numeroLigne++,
                            CodeProduit = ligneBL.CodeProduit,
                            Quantite = ligneBL.Quantite,
                            PrixUnitaire = ligneBL.PrixUnitaireHT,
                            PrixUnitaireHT = ligneBL.PrixUnitaireHT,
                            Remise = montantRemiseLigne,
                            TauxTVA = ligneBL.TauxTVA,
                            TauxRemise = ligneBL.TauxRemise,
                            TauxFodec = ligneBL.TauxFodec,
                            MontantRemise = montantRemiseLigne,
                            MontantHT = ligneBL.MontantHT,
                            MontantTVA = ligneBL.MontantTVA,
                            MontantFodec = ligneBL.MontantFodec,
                            MontantTTC = ligneBL.MontantTTC
                        };

                        facture.Lignes.Add(ligneFacture);

                        montantHT += ligneBL.MontantHT;
                        montantTVA += ligneBL.MontantTVA;
                        montantFodec += ligneBL.MontantFodec;
                        montantRemise += montantRemiseLigne;
                    }
                }

                // Marquer le BL comme facturé
                bl.Statut = "Facturé";
                bl.Facture = true;
                bl.NumeroFacture = numeroFacture;
                await _unitOfWork.BonsLivraison.UpdateAsync(bl);
            }

            // Appliquer la remise globale
            var remiseGlobale = montantHT * (request.TauxRemiseGlobale / 100);
            montantRemise += remiseGlobale;

            facture.MontantHT = montantHT - remiseGlobale;
            facture.MontantTVA = montantTVA;
            facture.MontantFodec = montantFodec;
            facture.MontantRemise = montantRemise;
            facture.MontantTTC = facture.MontantHT + montantTVA + montantFodec + facture.Timbre;

            // Gérer la RAS si le client est soumis
            var client = await _unitOfWork.Clients.GetByCodeAsync(codeClient!, codeEntreprise);
            if (client?.SoumisRAS == true)
            {
                var retenue = await _unitOfWork.RetenuesSource.GetByCodeAsync(1);
                if (retenue != null)
                {
                    facture.TauxRAS = retenue.Taux;
                    facture.MontantRAS = facture.MontantTTC * (retenue.Taux / 100);
                }
            }

            facture.NetAPayer = facture.MontantTTC - facture.MontantRAS;
            facture.APayer = facture.NetAPayer;
            facture.MontantApresRAS = facture.NetAPayer;
            facture.MontantRestant = facture.NetAPayer;
            facture.MontantRegle = 0;

            await _unitOfWork.FacturesClient.AddAsync(facture);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync();

            var createdFacture = await _unitOfWork.FacturesClient.GetByNumeroAsync(numeroFacture, codeEntreprise);
            return _mapper.Map<FactureClientDto>(createdFacture);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private static bool NumeroBonLivraisonContains(string? numeroBonLivraisonFacture, string numeroBonLivraison)
    {
        if (string.IsNullOrWhiteSpace(numeroBonLivraisonFacture))
        {
            return false;
        }

        return numeroBonLivraisonFacture
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(x => string.Equals(x, numeroBonLivraison, StringComparison.OrdinalIgnoreCase));
    }
}
