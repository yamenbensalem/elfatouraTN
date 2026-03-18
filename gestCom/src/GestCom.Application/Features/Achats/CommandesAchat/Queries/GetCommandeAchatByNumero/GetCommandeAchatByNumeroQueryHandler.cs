using AutoMapper;
using GestCom.Application.Common.Exceptions;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Achats.CommandesAchat.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Achats.CommandesAchat.Queries.GetCommandeAchatByNumero;

public class GetCommandeAchatByNumeroQueryHandler : IRequestHandler<GetCommandeAchatByNumeroQuery, CommandeAchatDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public GetCommandeAchatByNumeroQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<CommandeAchatDto> Handle(GetCommandeAchatByNumeroQuery request, CancellationToken cancellationToken)
    {
        var codeEntreprise = _tenantContext.CodeEntreprise 
            ?? throw new System.UnauthorizedAccessException("Code entreprise manquant");

        var commande = await _unitOfWork.CommandesAchat.GetByNumeroAsync(request.NumeroCommande, codeEntreprise);

        if (commande == null)
            throw new NotFoundException(nameof(CommandeAchat), request.NumeroCommande);

        return _mapper.Map<CommandeAchatDto>(commande);
    }
}
