using GestCom.Application.Features.Achats.Fournisseurs.DTOs;
using GestCom.Shared.Common;
using MediatR;

namespace GestCom.Application.Features.Achats.Fournisseurs.Queries.GetAllFournisseurs;

public class GetAllFournisseursQuery : IRequest<PagedResult<FournisseurListDto>>
{
    public string? SearchTerm { get; set; }
    public string? Ville { get; set; }
    public bool? AvecDettes { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "RaisonSociale";
    public bool SortDescending { get; set; } = false;
}
