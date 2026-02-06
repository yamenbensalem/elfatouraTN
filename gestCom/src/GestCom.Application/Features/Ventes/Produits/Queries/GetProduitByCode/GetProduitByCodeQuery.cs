using GestCom.Application.Features.Ventes.Produits.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.Produits.Queries.GetProduitByCode;

public class GetProduitByCodeQuery : IRequest<ProduitDto?>
{
    public string CodeProduit { get; set; } = string.Empty;
}
