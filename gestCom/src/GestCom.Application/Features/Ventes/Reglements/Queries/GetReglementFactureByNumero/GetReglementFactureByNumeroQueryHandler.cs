using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Ventes.Reglements.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Ventes.Reglements.Queries.GetReglementFactureByNumero;

public class GetReglementFactureByNumeroQueryHandler : IRequestHandler<GetReglementFactureByNumeroQuery, ReglementFactureDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetReglementFactureByNumeroQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<ReglementFactureDto?> Handle(GetReglementFactureByNumeroQuery request, CancellationToken cancellationToken)
    {
        var codeEntreprise = _currentUserService.CodeEntreprise
            ?? throw new InvalidOperationException("Code entreprise introuvable pour l'utilisateur courant.");

        if (!TryExtractId(request.NumeroReglement, out var reglementId))
        {
            return null;
        }

        var reglement = (await _unitOfWork.ReglementsFacture
                .FindAsync(r => r.Id == reglementId && r.CodeEntreprise == codeEntreprise))
            .FirstOrDefault();

        if (reglement == null)
        {
            return null;
        }

        if (request.NumeroReglement.Contains('-', StringComparison.Ordinal)
            && !string.Equals(BuildNumeroReglement(reglement), request.NumeroReglement, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var dto = _mapper.Map<ReglementFactureDto>(reglement);

        var facture = await _unitOfWork.FacturesClient.GetByNumeroAsync(reglement.NumeroFacture, codeEntreprise);
        if (facture != null)
        {
            dto.MontantFacture = facture.APayer > 0 ? facture.APayer : facture.NetAPayer;
            dto.ResteARegler = facture.MontantRestant;
            dto.NomClient ??= facture.Client?.Nom;
        }

        if (string.IsNullOrWhiteSpace(dto.NomClient))
        {
            var client = await _unitOfWork.Clients.GetByCodeAsync(reglement.CodeClient, codeEntreprise);
            dto.NomClient = client?.Nom;
        }

        return dto;
    }

    private static string BuildNumeroReglement(ReglementFacture reglement)
    {
        return $"REG-{reglement.DateReglement:yyyyMMdd}-{reglement.Id:D6}";
    }

    private static bool TryExtractId(string numeroReglement, out int id)
    {
        id = 0;

        if (string.IsNullOrWhiteSpace(numeroReglement))
        {
            return false;
        }

        var parts = numeroReglement.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var idPart = parts.Length > 0 ? parts[^1] : numeroReglement;

        return int.TryParse(idPart, out id);
    }
}