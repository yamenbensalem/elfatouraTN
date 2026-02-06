using AutoMapper;
using GestCom.Application.Features.Configuration.DTOs;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Configuration.Devises.Queries.GetAllDevises;

public class GetAllDevisesQueryHandler : IRequestHandler<GetAllDevisesQuery, IEnumerable<DeviseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllDevisesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DeviseDto>> Handle(GetAllDevisesQuery request, CancellationToken cancellationToken)
    {
        var devises = await _unitOfWork.Devises.GetAllAsync();
        return _mapper.Map<IEnumerable<DeviseDto>>(devises.OrderBy(d => d.LibelleDevise));
    }
}
