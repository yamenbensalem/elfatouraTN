using GestCom.Application.Features.Ventes.Produits.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.Produits.Queries.SearchProduits;

public class SearchProduitsQuery : IRequest<List<SearchProduitDto>>
{
    public string SearchTerm { get; set; } = string.Empty;
    public int MaxResults { get; set; } = 10;
}
