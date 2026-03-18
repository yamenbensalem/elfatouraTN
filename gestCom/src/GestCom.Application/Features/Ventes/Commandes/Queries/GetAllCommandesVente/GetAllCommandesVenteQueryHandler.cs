using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Ventes.Commandes.DTOs;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Common;
using MediatR;

namespace GestCom.Application.Features.Ventes.Commandes.Queries.GetAllCommandesVente;

public class GetAllCommandesVenteQueryHandler : IRequestHandler<GetAllCommandesVenteQuery, PagedResult<CommandeVenteListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public GetAllCommandesVenteQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<PagedResult<CommandeVenteListDto>> Handle(GetAllCommandesVenteQuery request, CancellationToken cancellationToken)
    {
        var codeEntreprise = _tenantContext.CodeEntreprise 
            ?? throw new System.UnauthorizedAccessException("Code entreprise manquant");

        var pagedResult = await _unitOfWork.CommandesVente.GetPagedCommandesVenteAsync(
            request.PageNumber,
            request.PageSize,
            codeEntreprise,
            request.CodeClient,
            request.Statut,
            request.DateDebut,
            request.DateFin
        );

        var dtos = _mapper.Map<IEnumerable<CommandeVenteListDto>>(pagedResult.Items);

        return PagedResult<CommandeVenteListDto>.Create(
            dtos.ToList(), 
            pagedResult.TotalCount, 
            pagedResult.PageNumber, 
            pagedResult.PageSize);
    }
}
