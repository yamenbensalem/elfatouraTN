using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Achats.CommandesAchat.DTOs;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Common;
using MediatR;

namespace GestCom.Application.Features.Achats.CommandesAchat.Queries.GetAllCommandesAchat;

public class GetAllCommandesAchatQueryHandler : IRequestHandler<GetAllCommandesAchatQuery, PagedResult<CommandeAchatListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public GetAllCommandesAchatQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<PagedResult<CommandeAchatListDto>> Handle(GetAllCommandesAchatQuery request, CancellationToken cancellationToken)
    {
        var codeEntreprise = _tenantContext.CodeEntreprise 
            ?? throw new System.UnauthorizedAccessException("Code entreprise manquant");

        var pagedResult = await _unitOfWork.CommandesAchat.GetPagedCommandesAchatAsync(
            request.PageNumber,
            request.PageSize,
            codeEntreprise,
            request.CodeFournisseur,
            request.Statut,
            request.DateDebut,
            request.DateFin
        );

        var dtos = _mapper.Map<IEnumerable<CommandeAchatListDto>>(pagedResult.Items);

        return PagedResult<CommandeAchatListDto>.Create(
            dtos.ToList(), 
            pagedResult.TotalCount, 
            pagedResult.PageNumber, 
            pagedResult.PageSize);
    }
}
