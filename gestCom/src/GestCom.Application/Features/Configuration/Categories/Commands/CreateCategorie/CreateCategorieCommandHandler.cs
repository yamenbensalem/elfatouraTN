using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Configuration.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Configuration.Categories.Commands.CreateCategorie;

public class CreateCategorieCommandHandler : IRequestHandler<CreateCategorieCommand, CategorieProduitDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public CreateCategorieCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<CategorieProduitDto> Handle(CreateCategorieCommand request, CancellationToken cancellationToken)
    {
        // Vérifier l'unicité du code
        if (!int.TryParse(request.CodeCategorie, out var codeCategorie))
        {
            throw new InvalidOperationException($"Le code catégorie '{request.CodeCategorie}' doit être un entier valide.");
        }
        
        var existing = await _unitOfWork.CategoriesProduit.GetByCodeAsync(codeCategorie, _currentUserService.CodeEntreprise);
        if (existing != null)
        {
            throw new InvalidOperationException($"Une catégorie avec le code '{request.CodeCategorie}' existe déjà.");
        }

        var categorie = new CategorieProduit
        {
            CodeEntreprise = _currentUserService.CodeEntreprise!,
            CodeCategorie = codeCategorie,
            Designation = request.LibelleCategorie ?? string.Empty,
            Description = request.Description,
            DateCreation = DateTime.Now
        };

        await _unitOfWork.CategoriesProduit.AddAsync(categorie);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CategorieProduitDto>(categorie);
    }
}
