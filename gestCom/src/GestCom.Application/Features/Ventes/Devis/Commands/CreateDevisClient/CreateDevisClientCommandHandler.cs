using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Ventes.Devis.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Exceptions;
using MediatR;

namespace GestCom.Application.Features.Ventes.Devis.Commands.CreateDevisClient;

public class CreateDevisClientCommandHandler : IRequestHandler<CreateDevisClientCommand, DevisClientDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public CreateDevisClientCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<DevisClientDto> Handle(CreateDevisClientCommand request, CancellationToken cancellationToken)
    {
        // Vérifier que le client existe
        var client = await _unitOfWork.Clients.GetByCodeAsync(request.CodeClient, _currentUserService.CodeEntreprise);
        if (client == null)
        {
            throw new NotFoundException("Client", request.CodeClient);
        }

        // Générer le numéro de devis
        var numeroDevis = await GenerateNumeroDevisAsync();

        // Créer le devis
        var devis = new DevisClient
        {
            NumeroDevis = numeroDevis,
            DateDevis = request.DateDevis,
            DateValidite = request.DateValidite ?? request.DateDevis.AddDays(30),
            CodeClient = request.CodeClient,
            TauxRemise = request.TauxRemise,
            Timbre = request.Timbre,
            CodeDevise = request.CodeDevise,
            TauxChange = request.TauxChange,
            Observations = request.Observations,
            Statut = "En cours"
        };

        // Ajouter les lignes
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

            var montantBrutHT = ligneDto.Quantite * ligneDto.PrixUnitaireHT;
            var montantRemise = montantBrutHT * (ligneDto.TauxRemise / 100);
            var montantNetHT = montantBrutHT - montantRemise;
            var montantTVA = montantNetHT * (ligneDto.TauxTVA / 100);
            var montantTTC = montantNetHT + montantTVA;

            var ligne = new LigneDevisClient
            {
                NumeroDevis = numeroDevis,
                NumeroLigne = numeroLigne++,
                CodeProduit = ligneDto.CodeProduit,
                Quantite = ligneDto.Quantite,
                PrixUnitaireHT = ligneDto.PrixUnitaireHT,
                TauxTVA = ligneDto.TauxTVA,
                TauxRemise = ligneDto.TauxRemise,
                MontantRemise = montantRemise,
                MontantHT = montantNetHT,
                MontantTVA = montantTVA,
                MontantTTC = montantTTC
            };

            devis.LignesDevis.Add(ligne);

            totalHT += montantNetHT;
            totalTVA += montantTVA;
        }

        // Appliquer la remise globale
        var remiseGlobale = totalHT * (request.TauxRemise / 100);
        totalHT -= remiseGlobale;

        devis.MontantHT = totalHT;
        devis.MontantTVA = totalTVA;
        devis.Remise = remiseGlobale;
        devis.MontantTTC = totalHT + totalTVA + request.Timbre;

        await _unitOfWork.DevisClients.AddAsync(devis);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdDevis = await _unitOfWork.DevisClients.GetByNumeroAsync(numeroDevis, _currentUserService.CodeEntreprise);
        return _mapper.Map<DevisClientDto>(createdDevis);
    }

    private async Task<string> GenerateNumeroDevisAsync()
    {
        var year = DateTime.Now.Year;
        var lastNumber = await _unitOfWork.DevisClients.GetLastNumeroAsync(_currentUserService.CodeEntreprise);
        
        int nextNumber = 1;
        if (!string.IsNullOrEmpty(lastNumber))
        {
            var parts = lastNumber.Split('-');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int last))
            {
                nextNumber = last + 1;
            }
        }

        return $"DV{year}-{nextNumber:D6}";
    }
}
