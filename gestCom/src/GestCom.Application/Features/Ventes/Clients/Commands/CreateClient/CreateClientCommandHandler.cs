using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Ventes.Clients.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Exceptions;
using MediatR;

namespace GestCom.Application.Features.Ventes.Clients.Commands.CreateClient;

/// <summary>
/// Handler pour la création d'un client
/// </summary>
public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, ClientDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public CreateClientCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<ClientDto> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        // Vérifier si le client existe déjà
        var existingClient = await _unitOfWork.Clients.GetByCodeAsync(request.CodeClient, _currentUserService.CodeEntreprise);
        if (existingClient != null)
        {
            throw new BusinessException($"Un client avec le code '{request.CodeClient}' existe déjà.");
        }

        // Créer l'entité
        var client = _mapper.Map<Client>(request);
        client.CodeEntreprise = _currentUserService.CodeEntreprise!;
        client.Etat = "Actif";
        client.NombreTransactions = 0;
        client.DateCreation = DateTime.Now;

        // Ajouter à la base de données
        await _unitOfWork.Clients.AddAsync(client);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapper et retourner le DTO
        var clientDto = _mapper.Map<ClientDto>(client);
        clientDto.TotalCreances = 0; // Nouveau client
        
        return clientDto;
    }
}
