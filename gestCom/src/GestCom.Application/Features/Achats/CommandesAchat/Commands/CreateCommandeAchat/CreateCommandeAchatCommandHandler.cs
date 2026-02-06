using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Achats.CommandesAchat.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Exceptions;
using MediatR;

namespace GestCom.Application.Features.Achats.CommandesAchat.Commands.CreateCommandeAchat;

public class CreateCommandeAchatCommandHandler : IRequestHandler<CreateCommandeAchatCommand, CommandeAchatDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public CreateCommandeAchatCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<CommandeAchatDto> Handle(CreateCommandeAchatCommand request, CancellationToken cancellationToken)
    {
        var codeEntreprise = _currentUserService.CodeEntreprise ?? request.CodeEntreprise;

        var fournisseur = await _unitOfWork.Fournisseurs.GetByCodeAsync(request.CodeFournisseur, codeEntreprise);
        if (fournisseur == null)
        {
            throw new NotFoundException("Fournisseur", request.CodeFournisseur);
        }

        var numeroCommande = await GenerateNumeroCommandeAsync(codeEntreprise);

        var commande = new CommandeAchat
        {
            NumeroCommande = numeroCommande,
            CodeEntreprise = codeEntreprise,
            DateCommande = request.DateCommande,
            DateLivraison = request.DateLivraison,
            CodeFournisseur = request.CodeFournisseur,
            Remise = request.Remise,
            Notes = request.Notes,
            Statut = "En cours",
            Lignes = new List<LigneCommandeAchat>()
        };

        decimal totalHT = 0;
        decimal totalTVA = 0;

        foreach (var ligneDto in request.Lignes)
        {
            var produit = await _unitOfWork.Produits.GetByCodeAsync(ligneDto.CodeProduit, codeEntreprise);
            if (produit == null)
            {
                throw new NotFoundException("Produit", ligneDto.CodeProduit);
            }

            var montantBrut = ligneDto.Quantite * ligneDto.PrixUnitaire;
            var remiseLigne = montantBrut * (ligneDto.Remise / 100);
            var montantHT = montantBrut - remiseLigne;
            var montantTVA = montantHT * (ligneDto.TauxTVA / 100);
            var montantTTC = montantHT + montantTVA;

            var ligne = new LigneCommandeAchat
            {
                NumeroCommande = numeroCommande,
                CodeProduit = ligneDto.CodeProduit,
                Designation = produit.Designation,
                Quantite = ligneDto.Quantite,
                PrixUnitaire = ligneDto.PrixUnitaire,
                Remise = ligneDto.Remise,
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

        await _unitOfWork.CommandesAchat.AddAsync(commande);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdCommande = await _unitOfWork.CommandesAchat.GetByNumeroAsync(numeroCommande, codeEntreprise);
        return _mapper.Map<CommandeAchatDto>(createdCommande);
    }

    private async Task<string> GenerateNumeroCommandeAsync(string codeEntreprise)
    {
        var year = DateTime.Now.Year;
        var commandes = await _unitOfWork.CommandesAchat.GetAllAsync();
        var lastNumber = commandes
            .Where(c => c.CodeEntreprise == codeEntreprise && c.NumeroCommande.StartsWith($"CA{year}"))
            .Select(c => c.NumeroCommande)
            .OrderByDescending(n => n)
            .FirstOrDefault();
        
        int nextNumber = 1;
        if (!string.IsNullOrEmpty(lastNumber))
        {
            var parts = lastNumber.Split('-');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int last))
            {
                nextNumber = last + 1;
            }
        }

        return $"CA{year}-{nextNumber:D6}";
    }
}
