using AutoMapper;
using GestCom.Application.Features.Ventes.Clients.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Common;
using MediatR;
using System.Linq.Expressions;

namespace GestCom.Application.Features.Ventes.Clients.Queries.GetAllClients;

/// <summary>
/// Handler pour obtenir la liste des clients
/// </summary>
public class GetAllClientsQueryHandler : IRequestHandler<GetAllClientsQuery, PagedResult<ClientListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllClientsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<ClientListDto>> Handle(GetAllClientsQuery request, CancellationToken cancellationToken)
    {
        // Construire le filtre
        Expression<Func<Client, bool>>? filter = null;

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            filter = c => c.CodeClient.Contains(request.SearchTerm) ||
                         c.Nom.Contains(request.SearchTerm) ||
                         c.MatriculeFiscale.Contains(request.SearchTerm) ||
                         (c.Email != null && c.Email.Contains(request.SearchTerm));
        }

        if (!string.IsNullOrEmpty(request.Etat))
        {
            var previousFilter = filter;
            filter = previousFilter == null
                ? c => c.Etat == request.Etat
                : c => (previousFilter.Compile()(c)) && c.Etat == request.Etat;
        }

        if (request.Etranger.HasValue)
        {
            var previousFilter = filter;
            filter = previousFilter == null
                ? c => c.Etranger == request.Etranger.Value
                : c => (previousFilter.Compile()(c)) && c.Etranger == request.Etranger.Value;
        }

        // Obtenir les clients avec pagination
        var pagedClients = await _unitOfWork.Clients.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            filter
        );

        // Mapper vers DTO
        var clientDtos = new List<ClientListDto>();
        foreach (var client in pagedClients.Items)
        {
            var dto = _mapper.Map<ClientListDto>(client);
            
            // Calculer le total des créances
            dto.TotalCreances = await _unitOfWork.Clients.GetTotalCreancesAsync(
                client.CodeClient,
                client.CodeEntreprise
            );
            
            clientDtos.Add(dto);
        }

        return PagedResult<ClientListDto>.Create(
            clientDtos,
            pagedClients.TotalCount,
            request.PageNumber,
            request.PageSize
        );
    }
}
