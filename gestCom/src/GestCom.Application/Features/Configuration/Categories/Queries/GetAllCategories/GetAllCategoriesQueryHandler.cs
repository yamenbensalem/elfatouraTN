using AutoMapper;
using GestCom.Application.Features.Configuration.DTOs;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Configuration.Categories.Queries.GetAllCategories;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, IEnumerable<CategorieProduitDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllCategoriesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategorieProduitDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.CategoriesProduit.GetAllAsync();
        var categoriesList = categories.ToList();

        // Filtrer par recherche
        if (!string.IsNullOrEmpty(request.Recherche))
        {
            var recherche = request.Recherche;
            categoriesList = categoriesList
                .Where(c => c.CodeCategorie.ToString().Contains(recherche, StringComparison.OrdinalIgnoreCase) ||
                           (c.LibelleCategorie != null && c.LibelleCategorie.Contains(recherche, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        // Récupérer les produits pour compter par catégorie
        var produits = await _unitOfWork.Produits.GetAllAsync();
        var produitsParCategorie = produits
            .Where(p => !string.IsNullOrEmpty(p.CodeCategorie))
            .GroupBy(p => p.CodeCategorie!)
            .ToDictionary(g => g.Key, g => g.Count());

        var result = _mapper.Map<List<CategorieProduitDto>>(categoriesList);

        // Ajouter le nombre de produits
        foreach (var cat in result)
        {
            cat.NombreProduits = produitsParCategorie.GetValueOrDefault(cat.CodeCategorie, 0);
        }

        return result.OrderBy(c => c.LibelleCategorie);
    }
}
