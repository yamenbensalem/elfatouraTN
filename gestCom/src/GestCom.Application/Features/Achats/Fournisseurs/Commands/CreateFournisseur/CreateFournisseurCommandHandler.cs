using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Achats.Fournisseurs.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using GestCom.Shared.Exceptions;
using MediatR;

namespace GestCom.Application.Features.Achats.Fournisseurs.Commands.CreateFournisseur;

public class CreateFournisseurCommandHandler : IRequestHandler<CreateFournisseurCommand, FournisseurDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public CreateFournisseurCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<FournisseurDto> Handle(CreateFournisseurCommand request, CancellationToken cancellationToken)
    {
        // Vérifier si le fournisseur existe déjà
        var existingFournisseur = await _unitOfWork.Fournisseurs.GetByCodeAsync(request.CodeFournisseur, _currentUserService.CodeEntreprise);
        if (existingFournisseur != null)
        {
            throw new BusinessException($"Un fournisseur avec le code '{request.CodeFournisseur}' existe déjà.");
        }

        // Vérifier unicité du matricule fiscal
        if (!string.IsNullOrEmpty(request.MatriculeFiscale))
        {
            var fournisseurByMatricule = await _unitOfWork.Fournisseurs.GetByMatriculeFiscaleAsync(request.MatriculeFiscale, _currentUserService.CodeEntreprise);
            if (fournisseurByMatricule != null)
            {
                throw new BusinessException($"Un fournisseur avec le matricule fiscal '{request.MatriculeFiscale}' existe déjà.");
            }
        }

        var fournisseur = _mapper.Map<Fournisseur>(request);
        fournisseur.CodeEntreprise = _currentUserService.CodeEntreprise!;
        fournisseur.DateCreation = DateTime.Now;

        await _unitOfWork.Fournisseurs.AddAsync(fournisseur);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<FournisseurDto>(fournisseur);
    }
}
