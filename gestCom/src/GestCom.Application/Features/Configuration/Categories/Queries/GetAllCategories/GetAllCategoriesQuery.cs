using GestCom.Application.Features.Configuration.DTOs;
using MediatR;

namespace GestCom.Application.Features.Configuration.Categories.Queries.GetAllCategories;

/// <summary>
/// Query pour récupérer toutes les catégories de produits
/// </summary>
public class GetAllCategoriesQuery : IRequest<IEnumerable<CategorieProduitDto>>
{
    /// <summary>
    /// Recherche par libellé
    /// </summary>
    public string? Recherche { get; set; }
}
