using AutoMapper;
using GestCom.Application.Features.Achats.ReglementsFournisseur.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Achats.ReglementsFournisseur.Commands.CreateReglementFournisseur;

public class CreateReglementFournisseurCommandHandler : IRequestHandler<CreateReglementFournisseurCommand, ReglementFournisseurDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateReglementFournisseurCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ReglementFournisseurDto> Handle(CreateReglementFournisseurCommand request, CancellationToken cancellationToken)
    {
        // Récupérer la facture
        var facture = await _unitOfWork.FacturesFournisseur.GetByNumeroAsync(request.NumeroFacture, request.CodeEntreprise);
        if (facture == null)
        {
            throw new InvalidOperationException($"Facture fournisseur '{request.NumeroFacture}' non trouvée.");
        }

        // Vérifier le montant restant à payer
        var resteAPayer = facture.MontantRestant;
        if (request.Montant > resteAPayer)
        {
            throw new InvalidOperationException(
                $"Le montant du règlement ({request.Montant:N3} TND) dépasse le reste à payer ({resteAPayer:N3} TND).");
        }

        // Créer le règlement
        var reglement = new ReglementFournisseur
        {
            CodeEntreprise = request.CodeEntreprise,
            DateReglement = request.DateReglement,
            NumeroFacture = request.NumeroFacture,
            CodeFournisseur = facture.CodeFournisseur,
            Montant = request.Montant,
            ModePayement = request.ModePayement ?? "Espèces",
            NumeroTransaction = request.NumeroTransaction,
            Notes = request.Notes
        };

        // Mettre à jour la facture
        facture.MontantRestant -= request.Montant;
        if (facture.MontantRestant <= 0)
        {
            facture.Statut = "Payée";
            facture.MontantRestant = 0;
        }
        else
        {
            facture.Statut = "Partiellement payée";
        }

        await _unitOfWork.ReglementsFournisseur.AddAsync(reglement);
        await _unitOfWork.FacturesFournisseur.UpdateAsync(facture);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Construire le DTO de retour
        var fournisseur = await _unitOfWork.Fournisseurs.GetByCodeAsync(facture.CodeFournisseur, request.CodeEntreprise);

        return new ReglementFournisseurDto
        {
            Id = reglement.Id,
            DateReglement = reglement.DateReglement,
            NumeroFacture = reglement.NumeroFacture,
            CodeFournisseur = reglement.CodeFournisseur,
            NomFournisseur = fournisseur?.Nom,
            Montant = reglement.Montant,
            ModePayement = reglement.ModePayement,
            NumeroTransaction = reglement.NumeroTransaction,
            Notes = reglement.Notes
        };
    }
}
