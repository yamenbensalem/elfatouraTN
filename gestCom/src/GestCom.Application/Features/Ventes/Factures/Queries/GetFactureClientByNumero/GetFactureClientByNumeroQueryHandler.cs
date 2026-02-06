using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Ventes.Factures.DTOs;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Ventes.Factures.Queries.GetFactureClientByNumero;

public class GetFactureClientByNumeroQueryHandler : IRequestHandler<GetFactureClientByNumeroQuery, FactureClientDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetFactureClientByNumeroQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<FactureClientDto?> Handle(GetFactureClientByNumeroQuery request, CancellationToken cancellationToken)
    {
        var facture = await _unitOfWork.FacturesClient.GetByNumeroAsync(request.NumeroFacture, _currentUserService.CodeEntreprise);
        if (facture == null)
        {
            return null;
        }

        return _mapper.Map<FactureClientDto>(facture);
    }
}
