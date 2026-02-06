using AutoMapper;
using GestCom.Application.Features.Configuration.DTOs;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Configuration.Unites.Queries.GetAllUnites;

public class GetAllUnitesQueryHandler : IRequestHandler<GetAllUnitesQuery, IEnumerable<UniteProduitDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllUnitesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UniteProduitDto>> Handle(GetAllUnitesQuery request, CancellationToken cancellationToken)
    {
        // UniteProduit repository doesn't exist in IUnitOfWork, need to add it or handle differently
        // For now, let's assume we need to get units from another source or the repository needs to be added
        return new List<UniteProduitDto>();
    }
}
