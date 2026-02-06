using System.Linq.Expressions;
using AutoMapper;
using GestCom.Application.Features.Ventes.Devis.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Common;
using MediatR;

namespace GestCom.Application.Features.Ventes.Devis.Queries.GetAllDevisClient;

public class GetAllDevisClientQueryHandler : IRequestHandler<GetAllDevisClientQuery, PagedResult<DevisClientListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllDevisClientQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<DevisClientListDto>> Handle(GetAllDevisClientQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<DevisClient, bool>>? filter = null;
        var filters = new List<Expression<Func<DevisClient, bool>>>();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            filters.Add(d => d.NumeroDevis.ToLower().Contains(searchTerm) ||
                           (d.Client != null && d.Client.RaisonSociale != null && d.Client.RaisonSociale.ToLower().Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(request.CodeClient))
        {
            filters.Add(d => d.CodeClient == request.CodeClient);
        }

        if (request.DateDebut.HasValue)
        {
            filters.Add(d => d.DateDevis >= request.DateDebut.Value);
        }

        if (request.DateFin.HasValue)
        {
            filters.Add(d => d.DateDevis <= request.DateFin.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Statut))
        {
            filters.Add(d => d.Statut == request.Statut);
        }

        if (request.NonExpiresOnly == true)
        {
            var today = DateTime.Now.Date;
            filters.Add(d => d.DateValidite == null || d.DateValidite >= today);
        }

        if (request.NonConvertisOnly == true)
        {
            filters.Add(d => string.IsNullOrEmpty(d.NumeroCommande));
        }

        if (filters.Any())
        {
            filter = filters.First();
            foreach (var f in filters.Skip(1))
            {
                var param = Expression.Parameter(typeof(DevisClient));
                var body = Expression.AndAlso(
                    Expression.Invoke(filter, param),
                    Expression.Invoke(f, param));
                filter = Expression.Lambda<Func<DevisClient, bool>>(body, param);
            }
        }

        var pagedResult = await _unitOfWork.DevisClients.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            filter,
            null,
            !request.SortDescending);

        var dtos = _mapper.Map<List<DevisClientListDto>>(pagedResult.Items);

        return new PagedResult<DevisClientListDto>(
            dtos,
            pagedResult.TotalCount,
            request.PageNumber,
            request.PageSize);
    }
}
