using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Reporting.DTOs;
using GestCom.Application.Features.Ventes.Factures.DTOs;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Ventes.Factures.Queries.GetChiffreAffaires;

public class GetChiffreAffairesQueryHandler : IRequestHandler<GetChiffreAffairesQuery, ChiffreAffairesDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetChiffreAffairesQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ChiffreAffairesDto> Handle(GetChiffreAffairesQuery request, CancellationToken cancellationToken)
    {
        var result = new ChiffreAffairesDto
        {
            DateDebut = request.DateDebut,
            DateFin = request.DateFin
        };

        // Récupérer les statistiques globales
        var chiffreAffaires = await _unitOfWork.FacturesClient.GetChiffreAffairesAsync(
            _currentUserService.CodeEntreprise, 
            request.DateDebut, 
            request.DateFin);

        result.TotalTTC = chiffreAffaires;

        // Récupérer les statistiques par mois si demandé
        if (request.IncludeParMois)
        {
            var statsParMois = await _unitOfWork.FacturesClient.GetChiffreAffairesParMoisAsync(
                _currentUserService.CodeEntreprise,
                request.DateDebut,
                request.DateFin);

            result.ParMois = statsParMois.Select(kvp => 
            {
                var parts = kvp.Key.Split('-');
                return new ChiffreAffairesParMoisDto
                {
                    Annee = parts.Length > 0 ? int.Parse(parts[0]) : 0,
                    Mois = parts.Length > 1 ? int.Parse(parts[1]) : 0,
                    MontantTTC = kvp.Value
                };
            }).ToList();
        }

        return result;
    }
}
