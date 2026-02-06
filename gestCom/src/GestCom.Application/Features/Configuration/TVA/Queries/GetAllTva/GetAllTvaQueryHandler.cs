using AutoMapper;
using GestCom.Application.Features.Configuration.DTOs;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Configuration.TVA.Queries.GetAllTva;

public class GetAllTvaQueryHandler : IRequestHandler<GetAllTvaQuery, IEnumerable<TvaProduitDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllTvaQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TvaProduitDto>> Handle(GetAllTvaQuery request, CancellationToken cancellationToken)
    {
        var tvas = await _unitOfWork.TVA.GetAllAsync();
        return _mapper.Map<IEnumerable<TvaProduitDto>>(tvas.OrderBy(t => t.Taux));
    }
}
