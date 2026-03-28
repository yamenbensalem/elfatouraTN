using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Ventes.Reglements.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Ventes.Reglements.Queries.GetAllReglementsFacture;

public class GetAllReglementsFactureQueryHandler : IRequestHandler<GetAllReglementsFactureQuery, IEnumerable<ReglementFactureListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetAllReglementsFactureQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<ReglementFactureListDto>> Handle(GetAllReglementsFactureQuery request, CancellationToken cancellationToken)
    {
        var codeEntreprise = _currentUserService.CodeEntreprise
            ?? throw new InvalidOperationException("Code entreprise introuvable pour l'utilisateur courant.");

        IEnumerable<ReglementFacture> reglements;

        if (!string.IsNullOrWhiteSpace(request.NumeroFacture))
        {
            reglements = await _unitOfWork.ReglementsFacture.GetReglementsByFactureAsync(request.NumeroFacture, codeEntreprise);
        }
        else if (!string.IsNullOrWhiteSpace(request.CodeClient))
        {
            reglements = await _unitOfWork.ReglementsFacture.GetReglementsByClientAsync(request.CodeClient, codeEntreprise);
        }
        else
        {
            reglements = await _unitOfWork.ReglementsFacture.GetAllAsync();
        }

        reglements = reglements.Where(r => r.CodeEntreprise == codeEntreprise);

        if (!string.IsNullOrWhiteSpace(request.CodeClient))
        {
            reglements = reglements.Where(r => r.CodeClient == request.CodeClient);
        }

        if (!string.IsNullOrWhiteSpace(request.NumeroFacture))
        {
            reglements = reglements.Where(r => r.NumeroFacture == request.NumeroFacture);
        }

        if (request.DateDebut.HasValue)
        {
            var dateDebut = request.DateDebut.Value.Date;
            reglements = reglements.Where(r => r.DateReglement.Date >= dateDebut);
        }

        if (request.DateFin.HasValue)
        {
            var dateFin = request.DateFin.Value.Date;
            reglements = reglements.Where(r => r.DateReglement.Date <= dateFin);
        }

        var reglementList = reglements
            .OrderByDescending(r => r.DateReglement)
            .ThenByDescending(r => r.Id)
            .ToList();

        var dtos = _mapper.Map<List<ReglementFactureListDto>>(reglementList);

        if (dtos.Count == 0)
        {
            return dtos;
        }

        var clientNames = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var codeClient in reglementList.Select(r => r.CodeClient).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var client = await _unitOfWork.Clients.GetByCodeAsync(codeClient, codeEntreprise);
            clientNames[codeClient] = client?.Nom;
        }

        for (var i = 0; i < dtos.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(dtos[i].NomClient)
                && clientNames.TryGetValue(reglementList[i].CodeClient, out var nomClient))
            {
                dtos[i].NomClient = nomClient;
            }
        }

        return dtos;
    }
}