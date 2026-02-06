using GestCom.Application.Features.Ventes.Produits.DTOs;
using GestCom.Shared.Common;
using MediatR;

namespace GestCom.Application.Features.Ventes.Produits.Queries.GetAllProduits;

public class GetAllProduitsQuery : IRequest<PagedResult<ProduitListDto>>
{
    public string? SearchTerm { get; set; }
    public string? CodeCategorie { get; set; }
    public string? CodeFournisseur { get; set; }
    public string? CodeMagasin { get; set; }
    public bool? StockFaibleOnly { get; set; }
    public decimal? PrixMin { get; set; }
    public decimal? PrixMax { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "Designation";
    public bool SortDescending { get; set; } = false;
}
