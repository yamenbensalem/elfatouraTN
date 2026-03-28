using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Ventes.Reglements.DTOs;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Ventes.Reglements.Queries.GetResumeReglementsClient;

public class GetResumeReglementsClientQueryHandler : IRequestHandler<GetResumeReglementsClientQuery, ResumeReglementsClientDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetResumeReglementsClientQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<ResumeReglementsClientDto?> Handle(GetResumeReglementsClientQuery request, CancellationToken cancellationToken)
    {
        var codeEntreprise = _currentUserService.CodeEntreprise
            ?? throw new InvalidOperationException("Code entreprise introuvable pour l'utilisateur courant.");

        var client = await _unitOfWork.Clients.GetByCodeAsync(request.CodeClient, codeEntreprise);
        if (client == null)
        {
            return null;
        }

        var factures = (await _unitOfWork.FacturesClient.GetFacturesByClientAsync(request.CodeClient, codeEntreprise)).ToList();
        var reglements = (await _unitOfWork.ReglementsFacture.GetReglementsByClientAsync(request.CodeClient, codeEntreprise)).ToList();

        var totalFactures = factures.Sum(f => f.APayer > 0 ? f.APayer : f.NetAPayer);
        var totalReglements = reglements.Sum(r => r.Montant);
        var soldeCreances = factures.Sum(f => f.MontantRestant);
        var nombreFacturesImpayees = factures.Count(f => f.MontantRestant > 0);

        var derniersReglements = reglements
            .OrderByDescending(r => r.DateReglement)
            .ThenByDescending(r => r.Id)
            .Take(10)
            .ToList();

        var derniersReglementsDtos = _mapper.Map<List<ReglementFactureListDto>>(derniersReglements);
        foreach (var dto in derniersReglementsDtos)
        {
            dto.NomClient ??= client.Nom;
        }

        return new ResumeReglementsClientDto
        {
            CodeClient = client.CodeClient,
            NomClient = client.Nom,
            TotalFactures = totalFactures,
            TotalReglements = totalReglements,
            SoldeCreances = soldeCreances,
            NombreFacturesImpayees = nombreFacturesImpayees,
            DerniersReglements = derniersReglementsDtos
        };
    }
}