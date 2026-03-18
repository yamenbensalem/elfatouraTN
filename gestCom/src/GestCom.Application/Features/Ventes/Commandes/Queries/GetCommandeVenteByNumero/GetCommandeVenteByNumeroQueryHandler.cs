using AutoMapper;
using GestCom.Application.Common.Exceptions;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Ventes.Commandes.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Ventes.Commandes.Queries.GetCommandeVenteByNumero;

public class GetCommandeVenteByNumeroQueryHandler : IRequestHandler<GetCommandeVenteByNumeroQuery, CommandeVenteDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    public GetCommandeVenteByNumeroQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<CommandeVenteDto> Handle(GetCommandeVenteByNumeroQuery request, CancellationToken cancellationToken)
    {
        var codeEntreprise = _tenantContext.CodeEntreprise 
            ?? throw new System.UnauthorizedAccessException("Code entreprise manquant");

        var commande = await _unitOfWork.CommandesVente.GetByNumeroAsync(request.NumeroCommande, codeEntreprise);

        if (commande == null)
            throw new NotFoundException(nameof(CommandeVente), request.NumeroCommande);

        return _mapper.Map<CommandeVenteDto>(commande);
    }
}
