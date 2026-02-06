using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Ventes.Clients.DTOs;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Ventes.Clients.Queries.GetClientByCode;

/// <summary>
/// Handler pour obtenir un client par son code
/// </summary>
public class GetClientByCodeQueryHandler : IRequestHandler<GetClientByCodeQuery, ClientDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetClientByCodeQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<ClientDto?> Handle(GetClientByCodeQuery request, CancellationToken cancellationToken)
    {
        var client = await _unitOfWork.Clients.GetByCodeAsync(request.CodeClient, _currentUserService.CodeEntreprise);
        
        if (client == null)
            return null;

        var clientDto = _mapper.Map<ClientDto>(client);
        
        // Calculer le total des créances
        clientDto.TotalCreances = await _unitOfWork.Clients.GetTotalCreancesAsync(
            request.CodeClient,
            _currentUserService.CodeEntreprise
        );

        return clientDto;
    }
}
