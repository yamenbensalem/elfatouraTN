using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Ventes.Clients.DTOs;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Exceptions;
using MediatR;

namespace GestCom.Application.Features.Ventes.Clients.Commands.UpdateClient;

/// <summary>
/// Handler pour la mise à jour d'un client
/// </summary>
public class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, ClientDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public UpdateClientCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<ClientDto> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        // Récupérer le client existant
        var client = await _unitOfWork.Clients.GetByCodeAsync(request.CodeClient, _currentUserService.CodeEntreprise);
        if (client == null)
        {
            throw new NotFoundException($"Client avec le code '{request.CodeClient}' non trouvé.");
        }

        // Mettre à jour les propriétés
        client.MatriculeFiscale = request.MatriculeFiscale;
        client.Nom = request.Nom;
        client.TypePersonne = request.TypePersonne;
        client.TypeEntreprise = request.TypeEntreprise;
        client.RIB = request.RIB;
        client.Adresse = request.Adresse;
        client.CodePostal = request.CodePostal;
        client.Ville = request.Ville;
        client.Pays = request.Pays;
        client.Tel = request.Tel;
        client.TelMobile = request.TelMobile;
        client.Fax = request.Fax;
        client.Email = request.Email;
        client.SiteWeb = request.SiteWeb;
        client.Etat = request.Etat;
        client.MaxCredit = request.MaxCredit;
        client.CodeDevise = request.CodeDevise;
        client.Responsable = request.Responsable;
        client.Note = request.Note;
        client.DateModification = DateTime.Now;

        // Sauvegarder les modifications
        await _unitOfWork.Clients.UpdateAsync(client);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapper et retourner le DTO
        var clientDto = _mapper.Map<ClientDto>(client);
        clientDto.TotalCreances = await _unitOfWork.Clients.GetTotalCreancesAsync(
            request.CodeClient,
            _currentUserService.CodeEntreprise
        );

        return clientDto;
    }
}
