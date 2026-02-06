using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Reporting.DTOs;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Reporting.Dashboard.Queries.GetDashboardData;

/// <summary>
/// Handler pour récupérer les données du tableau de bord
/// </summary>
public class GetDashboardDataQueryHandler : IRequestHandler<GetDashboardDataQuery, DashboardDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetDashboardDataQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<DashboardDto> Handle(GetDashboardDataQuery request, CancellationToken cancellationToken)
    {
        var codeEntreprise = _currentUserService.CodeEntreprise;
        var today = DateTime.Today;
        var debutMois = new DateTime(today.Year, today.Month, 1);
        var debutAnnee = new DateTime(today.Year, 1, 1);
        var debutMoisPrecedent = debutMois.AddMonths(-1);
        var finMoisPrecedent = debutMois.AddDays(-1);

        // Retourner un DTO avec des valeurs par défaut
        // L'implémentation complète nécessite les repositories spécialisés
        return new DashboardDto
        {
            ChiffreAffairesMois = 0,
            ChiffreAffairesAnnee = 0,
            ChiffreAffairesMoisPrecedent = 0,
            EvolutionCA = 0,
            TotalCreances = 0,
            FacturesImpayees = 0,
            TotalAchatsMois = 0,
            TotalDettes = 0,
            FacturesFournisseurImpayees = 0,
            ProduitsEnStock = 0,
            ProduitsStockFaible = 0,
            ValeurStock = 0,
            DevisAujourdhui = 0,
            CommandesAujourdhui = 0,
            FacturesAujourdhui = 0,
            BonsLivraisonAujourdhui = 0,
            TopClients = new List<TopClientDto>(),
            TopProduits = new List<TopProduitDto>(),
            CAParMois = new List<ChiffreAffairesParMoisDto>(),
            VentesParCategorie = new List<VentesParCategorieDto>()
        };
    }
}
