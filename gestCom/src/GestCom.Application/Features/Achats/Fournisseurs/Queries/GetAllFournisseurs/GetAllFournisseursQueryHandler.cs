using System.Linq.Expressions;
using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Achats.Fournisseurs.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Common;
using MediatR;

namespace GestCom.Application.Features.Achats.Fournisseurs.Queries.GetAllFournisseurs;

public class GetAllFournisseursQueryHandler : IRequestHandler<GetAllFournisseursQuery, PagedResult<FournisseurListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetAllFournisseursQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<FournisseurListDto>> Handle(GetAllFournisseursQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<Fournisseur, bool>>? filter = null;
        var filters = new List<Expression<Func<Fournisseur, bool>>>();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            filters.Add(f => (f.RaisonSociale != null && f.RaisonSociale.ToLower().Contains(searchTerm)) ||
                           f.CodeFournisseur.ToLower().Contains(searchTerm) ||
                           (f.MatriculeFiscale != null && f.MatriculeFiscale.ToLower().Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(request.Ville))
        {
            filters.Add(f => f.Ville == request.Ville);
        }

        if (filters.Any())
        {
            filter = filters.First();
            foreach (var f in filters.Skip(1))
            {
                var param = Expression.Parameter(typeof(Fournisseur));
                var body = Expression.AndAlso(
                    Expression.Invoke(filter, param),
                    Expression.Invoke(f, param));
                filter = Expression.Lambda<Func<Fournisseur, bool>>(body, param);
            }
        }

        var pagedResult = await _unitOfWork.Fournisseurs.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            filter,
            null,
            !request.SortDescending);

        var dtos = _mapper.Map<List<FournisseurListDto>>(pagedResult.Items);

        // Calculer les dettes pour chaque fournisseur
        foreach (var dto in dtos)
        {
            dto.TotalDettes = await _unitOfWork.Fournisseurs.GetTotalDettesAsync(dto.CodeFournisseur, _currentUserService.CodeEntreprise);
        }

        // Filtrer par dettes si demandé
        if (request.AvecDettes == true)
        {
            dtos = dtos.Where(d => d.TotalDettes > 0).ToList();
        }

        return new PagedResult<FournisseurListDto>(
            dtos,
            pagedResult.TotalCount,
            request.PageNumber,
            request.PageSize);
    }
}
