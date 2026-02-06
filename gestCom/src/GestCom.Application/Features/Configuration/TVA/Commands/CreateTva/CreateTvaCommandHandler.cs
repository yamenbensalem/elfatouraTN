using AutoMapper;
using GestCom.Application.Features.Configuration.DTOs;
using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Configuration.TVA.Commands.CreateTva;

public class CreateTvaCommandHandler : IRequestHandler<CreateTvaCommand, TvaProduitDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateTvaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<TvaProduitDto> Handle(CreateTvaCommand request, CancellationToken cancellationToken)
    {
        // Vérifier l'unicité du code
        var existing = await _unitOfWork.TVA.GetByIdAsync(request.CodeTVA);
        if (existing != null)
        {
            throw new InvalidOperationException($"Un taux de TVA avec le code '{request.CodeTVA}' existe déjà.");
        }

        var tva = new TvaProduit
        {
            CodeTVA = request.CodeTVA,
            Designation = request.Designation,
            Taux = request.Taux,
            ParDefaut = request.ParDefaut
        };

        await _unitOfWork.TVA.AddAsync(tva);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TvaProduitDto>(tva);
    }
}
