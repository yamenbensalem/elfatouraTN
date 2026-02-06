using System.Linq.Expressions;
using AutoMapper;
using GestCom.Application.Features.Ventes.Factures.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Common;
using MediatR;

namespace GestCom.Application.Features.Ventes.Factures.Queries.GetAllFacturesClient;

public class GetAllFacturesClientQueryHandler : IRequestHandler<GetAllFacturesClientQuery, PagedResult<FactureClientListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllFacturesClientQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<FactureClientListDto>> Handle(GetAllFacturesClientQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<FactureClient, bool>>? filter = null;
        var filters = new List<Expression<Func<FactureClient, bool>>>();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            filters.Add(f => f.NumeroFacture.ToLower().Contains(searchTerm) ||
                           (f.Client != null && f.Client.RaisonSociale != null && f.Client.RaisonSociale.ToLower().Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(request.CodeClient))
        {
            filters.Add(f => f.CodeClient == request.CodeClient);
        }

        if (request.DateDebut.HasValue)
        {
            filters.Add(f => f.DateFacture >= request.DateDebut.Value);
        }

        if (request.DateFin.HasValue)
        {
            filters.Add(f => f.DateFacture <= request.DateFin.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Statut))
        {
            filters.Add(f => f.Statut == request.Statut);
        }

        if (request.NonPayeesOnly == true)
        {
            filters.Add(f => f.MontantRegle < f.NetAPayer);
        }

        // Combiner les filtres
        if (filters.Any())
        {
            filter = filters.First();
            foreach (var f in filters.Skip(1))
            {
                var param = Expression.Parameter(typeof(FactureClient));
                var body = Expression.AndAlso(
                    Expression.Invoke(filter, param),
                    Expression.Invoke(f, param));
                filter = Expression.Lambda<Func<FactureClient, bool>>(body, param);
            }
        }

        var pagedResult = await _unitOfWork.FacturesClient.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            filter,
            null,
            !request.SortDescending);

        var dtos = _mapper.Map<List<FactureClientListDto>>(pagedResult.Items);

        return new PagedResult<FactureClientListDto>(
            dtos,
            pagedResult.TotalCount,
            request.PageNumber,
            request.PageSize);
    }
}
