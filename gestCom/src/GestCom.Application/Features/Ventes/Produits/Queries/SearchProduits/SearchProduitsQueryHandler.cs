using AutoMapper;
using GestCom.Application.Common.Interfaces;
using GestCom.Application.Features.Ventes.Produits.DTOs;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Ventes.Produits.Queries.SearchProduits;

public class SearchProduitsQueryHandler : IRequestHandler<SearchProduitsQuery, List<SearchProduitDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public SearchProduitsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<List<SearchProduitDto>> Handle(SearchProduitsQuery request, CancellationToken cancellationToken)
    {
        var produits = await _unitOfWork.Produits.SearchProduitsAsync(_currentUserService.CodeEntreprise, request.SearchTerm);
        return _mapper.Map<List<SearchProduitDto>>(produits);
    }
}
