using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Ventes.Reglements.DTOs;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Ventes.Reglements.Queries.GetReglementsFactureByFacture;

public class GetReglementsFactureByFactureQueryHandler : IRequestHandler<GetReglementsFactureByFactureQuery, IEnumerable<ReglementFactureDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetReglementsFactureByFactureQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<ReglementFactureDto>> Handle(GetReglementsFactureByFactureQuery request, CancellationToken cancellationToken)
    {
        var codeEntreprise = _currentUserService.CodeEntreprise
            ?? throw new InvalidOperationException("Code entreprise introuvable pour l'utilisateur courant.");

        var reglements = (await _unitOfWork.ReglementsFacture.GetReglementsByFactureAsync(request.NumeroFacture, codeEntreprise))
            .OrderByDescending(r => r.DateReglement)
            .ThenByDescending(r => r.Id)
            .ToList();

        var dtos = _mapper.Map<List<ReglementFactureDto>>(reglements);
        if (dtos.Count == 0)
        {
            return dtos;
        }

        var facture = await _unitOfWork.FacturesClient.GetByNumeroAsync(request.NumeroFacture, codeEntreprise);
        var montantFacture = facture != null
            ? (facture.APayer > 0 ? facture.APayer : facture.NetAPayer)
            : 0m;
        var resteARegler = facture?.MontantRestant ?? 0m;
        var nomClient = facture?.Client?.Nom;

        if (string.IsNullOrWhiteSpace(nomClient) && !string.IsNullOrWhiteSpace(facture?.CodeClient))
        {
            var client = await _unitOfWork.Clients.GetByCodeAsync(facture.CodeClient, codeEntreprise);
            nomClient = client?.Nom;
        }

        foreach (var dto in dtos)
        {
            dto.MontantFacture = montantFacture;
            dto.ResteARegler = resteARegler;
            dto.NomClient ??= nomClient;
        }

        return dtos;
    }
}