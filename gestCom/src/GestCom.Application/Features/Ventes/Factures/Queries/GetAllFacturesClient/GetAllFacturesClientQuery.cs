using GestCom.Application.Features.Ventes.Factures.DTOs;
using GestCom.Shared.Common;
using MediatR;

namespace GestCom.Application.Features.Ventes.Factures.Queries.GetAllFacturesClient;

public class GetAllFacturesClientQuery : IRequest<PagedResult<FactureClientListDto>>
{
    public string? SearchTerm { get; set; }
    public string? CodeClient { get; set; }
    public DateTime? DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
    public string? Statut { get; set; }
    public bool? NonPayeesOnly { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "DateFacture";
    public bool SortDescending { get; set; } = true;
}
