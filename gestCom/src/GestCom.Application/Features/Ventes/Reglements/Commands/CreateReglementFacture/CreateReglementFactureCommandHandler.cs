using AutoMapper;
using GestCom.Application.Features.Ventes.Reglements.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Exceptions;
using MediatR;

namespace GestCom.Application.Features.Ventes.Reglements.Commands.CreateReglementFacture;

public class CreateReglementFactureCommandHandler : IRequestHandler<CreateReglementFactureCommand, ReglementFactureDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateReglementFactureCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ReglementFactureDto> Handle(CreateReglementFactureCommand request, CancellationToken cancellationToken)
    {
        // Vérifier que la facture existe
        var facture = await _unitOfWork.FacturesClient.GetByNumeroAsync(request.NumeroFacture, request.CodeEntreprise);
        if (facture == null)
        {
            throw new NotFoundException("Facture", request.NumeroFacture);
        }

        // Calculer le reste à payer
        var resteAPayer = facture.MontantRestant;
        
        if (resteAPayer <= 0)
        {
            throw new BusinessException($"La facture '{request.NumeroFacture}' est déjà entièrement réglée.");
        }

        if (request.Montant > resteAPayer)
        {
            throw new BusinessException($"Le montant du règlement ({request.Montant:N3} TND) dépasse le reste à payer ({resteAPayer:N3} TND).");
        }

        if (request.Montant <= 0)
        {
            throw new BusinessException("Le montant du règlement doit être supérieur à zéro.");
        }

        var reglement = new ReglementFacture
        {
            CodeEntreprise = request.CodeEntreprise,
            DateReglement = request.DateReglement,
            NumeroFacture = request.NumeroFacture,
            CodeClient = facture.CodeClient,
            Montant = request.Montant,
            ModePayement = request.ModePayement ?? "Espèces",
            NumeroTransaction = request.NumeroTransaction,
            Notes = request.Notes
        };

        await _unitOfWork.ReglementsFacture.AddAsync(reglement);

        // Mettre à jour le montant restant de la facture
        facture.MontantRestant -= request.Montant;
        
        // Mettre à jour le statut de la facture
        if (facture.MontantRestant <= 0)
        {
            facture.Statut = "Payée";
            facture.MontantRestant = 0;
        }
        else
        {
            facture.Statut = "Partiellement payée";
        }

        await _unitOfWork.FacturesClient.UpdateAsync(facture);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Retourner le DTO
        var dto = _mapper.Map<ReglementFactureDto>(reglement);
        dto.NomClient = facture.Client?.Nom;
        dto.MontantFacture = facture.APayer;
        dto.ResteARegler = facture.MontantRestant;

        return dto;
    }
}
