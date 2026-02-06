using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Ventes.Produits.DTOs;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Ventes.Produits.Queries.GetProduitByCode;

public class GetProduitByCodeQueryHandler : IRequestHandler<GetProduitByCodeQuery, ProduitDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetProduitByCodeQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<ProduitDto?> Handle(GetProduitByCodeQuery request, CancellationToken cancellationToken)
    {
        var produit = await _unitOfWork.Produits.GetByCodeAsync(request.CodeProduit, _currentUserService.CodeEntreprise);
        if (produit == null)
        {
            return null;
        }

        return _mapper.Map<ProduitDto>(produit);
    }
}
