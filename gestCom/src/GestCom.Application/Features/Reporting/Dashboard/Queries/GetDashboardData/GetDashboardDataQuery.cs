using GestCom.Application.Features.Reporting.DTOs;
using MediatR;

namespace GestCom.Application.Features.Reporting.Dashboard.Queries.GetDashboardData;

/// <summary>
/// Query pour récupérer les données du tableau de bord
/// </summary>
public class GetDashboardDataQuery : IRequest<DashboardDto>
{
    /// <summary>
    /// Date de début de la période
    /// </summary>
    public DateTime DateDebut { get; set; } = DateTime.Today.AddMonths(-1);
    
    /// <summary>
    /// Date de fin de la période
    /// </summary>
    public DateTime DateFin { get; set; } = DateTime.Today;
    
    /// <summary>
    /// Nombre de top clients à retourner
    /// </summary>
    public int NombreTopClients { get; set; } = 5;
    
    /// <summary>
    /// Nombre de top produits à retourner
    /// </summary>
    public int NombreTopProduits { get; set; } = 5;
}
