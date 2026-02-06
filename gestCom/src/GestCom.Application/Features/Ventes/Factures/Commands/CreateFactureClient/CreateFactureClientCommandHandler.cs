using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Ventes.Factures.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Exceptions;
using MediatR;

namespace GestCom.Application.Features.Ventes.Factures.Commands.CreateFactureClient;

public class CreateFactureClientCommandHandler : IRequestHandler<CreateFactureClientCommand, FactureClientDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public CreateFactureClientCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<FactureClientDto> Handle(CreateFactureClientCommand request, CancellationToken cancellationToken)
    {
        // Vérifier que le client existe
        var client = await _unitOfWork.Clients.GetByCodeAsync(request.CodeClient, _currentUserService.CodeEntreprise);
        if (client == null)
        {
            throw new NotFoundException("Client", request.CodeClient);
        }

        // Vérifier le crédit maximum
        var totalCreances = await _unitOfWork.Clients.GetTotalCreancesAsync(request.CodeClient, _currentUserService.CodeEntreprise);
        var estimatedTotal = request.Lignes.Sum(l => l.Quantite * l.PrixUnitaireHT * (1 + l.TauxTVA / 100));
        
        if (client.CreditMaximum > 0 && (totalCreances + estimatedTotal) > client.CreditMaximum)
        {
            throw new BusinessException($"Le crédit maximum du client ({client.CreditMaximum:N3} TND) sera dépassé avec cette facture.");
        }

        // Générer le numéro de facture
        var numeroFacture = await GenerateNumeroFactureAsync();

        // Créer la facture
        var facture = new FactureClient
        {
            CodeEntreprise = _currentUserService.CodeEntreprise!,
            NumeroFacture = numeroFacture,
            DateFacture = request.DateFacture,
            DateEcheance = request.DateEcheance ?? request.DateFacture.AddDays(30),
            CodeClient = request.CodeClient,
            TauxRemise = request.TauxRemise,
            Timbre = request.Timbre,
            TauxRAS = request.TauxRAS,
            CodeDevise = request.CodeDevise,
            TauxChange = request.TauxChange,
            CodeModePaiement = request.CodeModePaiement,
            NumeroBonLivraison = request.NumeroBonLivraison,
            NumeroCommande = request.NumeroCommande,
            Observations = request.Observations,
            Statut = "Brouillon",
            MontantRegle = 0
        };

        // Ajouter les lignes et calculer les totaux
        int numeroLigne = 1;
        decimal totalHT = 0;
        decimal totalTVA = 0;
        decimal totalFODEC = 0;

        foreach (var ligneDto in request.Lignes)
        {
            // Vérifier que le produit existe
            var produit = await _unitOfWork.Produits.GetByCodeAsync(ligneDto.CodeProduit, _currentUserService.CodeEntreprise);
            if (produit == null)
            {
                throw new NotFoundException("Produit", ligneDto.CodeProduit);
            }

            // Vérifier le stock disponible
            if (produit.Quantite < ligneDto.Quantite)
            {
                throw new BusinessException($"Stock insuffisant pour le produit '{produit.Designation}'. Disponible: {produit.Quantite}, Demandé: {ligneDto.Quantite}");
            }

            // Calculer les montants de la ligne
            var montantBrutHT = ligneDto.Quantite * ligneDto.PrixUnitaireHT;
            var montantRemise = montantBrutHT * (ligneDto.TauxRemise / 100);
            var montantNetHT = montantBrutHT - montantRemise;
            var montantTVA = montantNetHT * (ligneDto.TauxTVA / 100);
            var montantFODEC = montantNetHT * (ligneDto.TauxFODEC / 100);
            var montantTTC = montantNetHT + montantTVA + montantFODEC;

            var ligne = new LigneFactureClient
            {
                NumeroFacture = numeroFacture,
                NumeroLigne = numeroLigne++,
                CodeProduit = ligneDto.CodeProduit,
                Quantite = ligneDto.Quantite,
                PrixUnitaireHT = ligneDto.PrixUnitaireHT,
                TauxTVA = ligneDto.TauxTVA,
                TauxRemise = ligneDto.TauxRemise,
                TauxFODEC = ligneDto.TauxFODEC,
                MontantRemise = montantRemise,
                MontantHT = montantNetHT,
                MontantTVA = montantTVA,
                MontantFODEC = montantFODEC,
                MontantTTC = montantTTC
            };

            facture.LignesFacture.Add(ligne);

            totalHT += montantNetHT;
            totalTVA += montantTVA;
            totalFODEC += montantFODEC;

            // Mettre à jour le stock du produit
            produit.Quantite -= ligneDto.Quantite;
            _unitOfWork.Produits.Update(produit);
        }

        // Appliquer la remise globale
        var remiseGlobale = totalHT * (request.TauxRemise / 100);
        totalHT -= remiseGlobale;

        // Calculer les totaux de la facture
        facture.MontantHT = totalHT;
        facture.MontantTVA = totalTVA;
        facture.MontantFODEC = totalFODEC;
        facture.Remise = remiseGlobale;
        facture.MontantTTC = totalHT + totalTVA + totalFODEC + request.Timbre;

        // Calculer la retenue à la source
        facture.MontantRAS = facture.MontantTTC * (request.TauxRAS / 100);
        facture.NetAPayer = facture.MontantTTC - facture.MontantRAS;

        await _unitOfWork.FacturesClient.AddAsync(facture);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Recharger avec les relations
        var createdFacture = await _unitOfWork.FacturesClient.GetByNumeroAsync(numeroFacture, _currentUserService.CodeEntreprise);
        return _mapper.Map<FactureClientDto>(createdFacture);
    }

    private async Task<string> GenerateNumeroFactureAsync()
    {
        var year = DateTime.Now.Year;
        var lastNumber = await _unitOfWork.FacturesClient.GetLastNumeroAsync(_currentUserService.CodeEntreprise);
        
        int nextNumber = 1;
        if (!string.IsNullOrEmpty(lastNumber))
        {
            var parts = lastNumber.Split('-');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int last))
            {
                nextNumber = last + 1;
            }
        }

        return $"FC{year}-{nextNumber:D6}";
    }
}
