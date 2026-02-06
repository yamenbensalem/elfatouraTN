using AutoMapper;
using GestCom.Application.Features.Ventes.Clients.DTOs;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Ventes.Clients.Queries.GetClientById;

/// <summary>
/// Handler pour obtenir un client par ID
/// </summary>
public class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, ClientDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetClientByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ClientDto?> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var client = await _unitOfWork.Clients.GetByCodeAsync(request.CodeClient, request.CodeEntreprise);
        
        if (client == null)
            return null;

        var clientDto = _mapper.Map<ClientDto>(client);
        
        // Calculer le total des créances
        clientDto.TotalCreances = await _unitOfWork.Clients.GetTotalCreancesAsync(
            request.CodeClient,
            request.CodeEntreprise
        );

        return clientDto;
    }
}
