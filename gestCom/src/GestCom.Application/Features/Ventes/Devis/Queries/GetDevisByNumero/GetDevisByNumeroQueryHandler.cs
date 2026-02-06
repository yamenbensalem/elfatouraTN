using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Ventes.Devis.DTOs;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Ventes.Devis.Queries.GetDevisByNumero;

/// <summary>
/// Handler pour obtenir un devis par son numéro
/// </summary>
public class GetDevisByNumeroQueryHandler : IRequestHandler<GetDevisByNumeroQuery, DevisClientDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetDevisByNumeroQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<DevisClientDto?> Handle(GetDevisByNumeroQuery request, CancellationToken cancellationToken)
    {
        var devis = await _unitOfWork.DevisClients.GetByNumeroAsync(request.NumeroDevis, _currentUserService.CodeEntreprise);
        
        if (devis == null)
            return null;

        return _mapper.Map<DevisClientDto>(devis);
    }
}
