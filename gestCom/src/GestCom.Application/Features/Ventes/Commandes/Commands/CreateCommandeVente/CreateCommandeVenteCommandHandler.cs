using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Ventes.Commandes.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Exceptions;
using MediatR;

namespace GestCom.Application.Features.Ventes.Commandes.Commands.CreateCommandeVente;

public class CreateCommandeVenteCommandHandler : IRequestHandler<CreateCommandeVenteCommand, CommandeVenteDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public CreateCommandeVenteCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<CommandeVenteDto> Handle(CreateCommandeVenteCommand request, CancellationToken cancellationToken)
    {
        // Vérifier que le client existe
        var client = await _unitOfWork.Clients.GetByCodeAsync(request.CodeClient, _currentUserService.CodeEntreprise);
        if (client == null)
        {
            throw new NotFoundException("Client", request.CodeClient);
        }

        // Si un devis est référencé, vérifier qu'il existe et n'est pas déjà converti
        if (!string.IsNullOrEmpty(request.NumeroDevis))
        {
            var devis = await _unitOfWork.DevisClients.GetByNumeroAsync(request.NumeroDevis, _currentUserService.CodeEntreprise);
            if (devis == null)
            {
                throw new NotFoundException("Devis", request.NumeroDevis);
            }
            if (!string.IsNullOrEmpty(devis.NumeroCommande))
            {
                throw new BusinessException($"Le devis '{request.NumeroDevis}' a déjà été converti en commande '{devis.NumeroCommande}'.");
            }
        }

        var numeroCommande = await GenerateNumeroCommandeAsync();

        var commande = new CommandeVente
        {
            CodeEntreprise = _currentUserService.CodeEntreprise!,
            NumeroCommande = numeroCommande,
            DateCommande = request.DateCommande,
            DateLivraisonPrevue = request.DateLivraisonPrevue,
            CodeClient = request.CodeClient,
            AdresseLivraison = request.AdresseLivraison ?? client.Adresse,
            TauxRemise = request.TauxRemise,
            CodeDevise = request.CodeDevise,
            TauxChange = request.TauxChange,
            NumeroDevis = request.NumeroDevis,
            Observations = request.Observations,
            Statut = "En cours"
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

            var montantBrutHT = ligneDto.Quantite * ligneDto.PrixUnitaireHT;
            var montantRemise = montantBrutHT * (ligneDto.TauxRemise / 100);
            var montantNetHT = montantBrutHT - montantRemise;
            var montantTVA = montantNetHT * (ligneDto.TauxTVA / 100);
            var montantTTC = montantNetHT + montantTVA;

            var ligne = new LigneCommandeVente
            {
                NumeroCommande = numeroCommande,
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

            commande.LignesCommande.Add(ligne);

            totalHT += montantNetHT;
            totalTVA += montantTVA;
        }

        var remiseGlobale = totalHT * (request.TauxRemise / 100);
        totalHT -= remiseGlobale;

        commande.MontantHT = totalHT;
        commande.MontantTVA = totalTVA;
        commande.Remise = remiseGlobale;
        commande.MontantTTC = totalHT + totalTVA;

        await _unitOfWork.CommandesVente.AddAsync(commande);

        // Mettre à jour le devis si référencé
        if (!string.IsNullOrEmpty(request.NumeroDevis))
        {
            var devis = await _unitOfWork.DevisClients.GetByNumeroAsync(request.NumeroDevis, _currentUserService.CodeEntreprise);
            if (devis != null)
            {
                devis.NumeroCommande = numeroCommande;
                devis.Statut = "Converti";
                _unitOfWork.DevisClients.Update(devis);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdCommande = await _unitOfWork.CommandesVente.GetByNumeroAsync(numeroCommande, _currentUserService.CodeEntreprise);
        return _mapper.Map<CommandeVenteDto>(createdCommande);
    }

    private async Task<string> GenerateNumeroCommandeAsync()
    {
        var year = DateTime.Now.Year;
        var lastNumber = await _unitOfWork.CommandesVente.GetLastNumeroAsync(_currentUserService.CodeEntreprise);
        
        int nextNumber = 1;
        if (!string.IsNullOrEmpty(lastNumber))
        {
            var parts = lastNumber.Split('-');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int last))
            {
                nextNumber = last + 1;
            }
        }

        return $"CV{year}-{nextNumber:D6}";
    }
}
