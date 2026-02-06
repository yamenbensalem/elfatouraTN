using GestCom.Application.Features.Ventes.Devis.DTOs;
using GestCom.Shared.Common;
using MediatR;

namespace GestCom.Application.Features.Ventes.Devis.Queries.GetAllDevisClient;

public class GetAllDevisClientQuery : IRequest<PagedResult<DevisClientListDto>>
{
    public string? SearchTerm { get; set; }
    public string? CodeClient { get; set; }
    public DateTime? DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
    public string? Statut { get; set; }
    public bool? NonExpiresOnly { get; set; }
    public bool? NonConvertisOnly { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "DateDevis";
    public bool SortDescending { get; set; } = true;
}
