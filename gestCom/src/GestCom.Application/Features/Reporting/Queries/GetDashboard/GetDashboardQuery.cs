using GestCom.Application.Features.Reporting.DTOs;
using MediatR;

namespace GestCom.Application.Features.Reporting.Queries.GetDashboard;

/// <summary>
/// Query pour récupérer les données du tableau de bord
/// </summary>
public class GetDashboardQuery : IRequest<DashboardDto>
{
    /// <summary>
    /// Date de référence (par défaut aujourd'hui)
    /// </summary>
    public DateTime? DateReference { get; set; }
    
    /// <summary>
    /// Nombre de top clients à retourner
    /// </summary>
    public int NombreTopClients { get; set; } = 5;
    
    /// <summary>
    /// Nombre de top produits à retourner
    /// </summary>
    public int NombreTopProduits { get; set; } = 5;
}
