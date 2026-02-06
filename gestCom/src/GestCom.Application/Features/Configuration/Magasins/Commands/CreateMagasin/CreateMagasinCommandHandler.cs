using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Configuration.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Exceptions;
using MediatR;

namespace GestCom.Application.Features.Configuration.Magasins.Commands.CreateMagasin;

/// <summary>
/// Handler pour la création d'un magasin
/// </summary>
public class CreateMagasinCommandHandler : IRequestHandler<CreateMagasinCommand, MagasinProduitDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public CreateMagasinCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<MagasinProduitDto> Handle(CreateMagasinCommand request, CancellationToken cancellationToken)
    {
        // Créer l'entité
        var magasin = new MagasinProduit
        {
            CodeEntreprise = _currentUserService.CodeEntreprise,
            Designation = request.LibelleMagasin,
            Adresse = request.Adresse,
            Responsable = request.Responsable,
            Principal = request.EstDefaut
        };

        // Ajouter à la base de données
        await _unitOfWork.MagasinsProduit.AddAsync(magasin);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapper et retourner le DTO
        return new MagasinProduitDto
        {
            CodeMagasin = magasin.CodeMagasin.ToString(),
            LibelleMagasin = magasin.Designation,
            Adresse = magasin.Adresse,
            Responsable = magasin.Responsable,
            EstDefaut = magasin.Principal,
            EstActif = true
        };
    }
}
