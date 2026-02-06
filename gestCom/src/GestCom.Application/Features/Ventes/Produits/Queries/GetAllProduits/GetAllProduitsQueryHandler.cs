using System.Linq.Expressions;
using AutoMapper;
using GestCom.Application.Features.Ventes.Produits.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Common;
using MediatR;

namespace GestCom.Application.Features.Ventes.Produits.Queries.GetAllProduits;

public class GetAllProduitsQueryHandler : IRequestHandler<GetAllProduitsQuery, PagedResult<ProduitListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllProduitsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProduitListDto>> Handle(GetAllProduitsQuery request, CancellationToken cancellationToken)
    {
        // Construire le filtre
        Expression<Func<Produit, bool>>? filter = null;
        var filters = new List<Expression<Func<Produit, bool>>>();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            filters.Add(p => p.Designation != null && p.Designation.ToLower().Contains(searchTerm) ||
                           p.CodeProduit.ToLower().Contains(searchTerm) ||
                           (p.CodeBarre != null && p.CodeBarre.ToLower().Contains(searchTerm)) ||
                           (p.Reference != null && p.Reference.ToLower().Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(request.CodeCategorie))
        {
            filters.Add(p => p.CodeCategorie == request.CodeCategorie);
        }

        if (!string.IsNullOrWhiteSpace(request.CodeFournisseur))
        {
            filters.Add(p => p.CodeFournisseur == request.CodeFournisseur);
        }

        if (!string.IsNullOrWhiteSpace(request.CodeMagasin))
        {
            filters.Add(p => p.CodeMagasin == request.CodeMagasin);
        }

        if (request.StockFaibleOnly == true)
        {
            filters.Add(p => p.Quantite <= p.StockMinimal);
        }

        if (request.PrixMin.HasValue)
        {
            filters.Add(p => p.PrixVenteTTC >= request.PrixMin.Value);
        }

        if (request.PrixMax.HasValue)
        {
            filters.Add(p => p.PrixVenteTTC <= request.PrixMax.Value);
        }

        // Combiner les filtres
        if (filters.Any())
        {
            filter = filters.First();
            foreach (var f in filters.Skip(1))
            {
                var param = Expression.Parameter(typeof(Produit));
                var body = Expression.AndAlso(
                    Expression.Invoke(filter, param),
                    Expression.Invoke(f, param));
                filter = Expression.Lambda<Func<Produit, bool>>(body, param);
            }
        }

        // Récupérer les données paginées
        var pagedProduits = await _unitOfWork.Produits.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            filter,
            null,
            !request.SortDescending);

        // Mapper vers DTOs
        var produitDtos = _mapper.Map<List<ProduitListDto>>(pagedProduits.Items);

        return new PagedResult<ProduitListDto>(
            produitDtos,
            pagedProduits.TotalCount,
            request.PageNumber,
            request.PageSize);
    }
}
