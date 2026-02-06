using AutoMapper;
using GestCom.Application.Features.Configuration.DTOs;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Configuration.ModesPaiement.Queries.GetAllModesPaiement;

public class GetAllModesPaiementQueryHandler : IRequestHandler<GetAllModesPaiementQuery, IEnumerable<ModePaiementDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllModesPaiementQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ModePaiementDto>> Handle(GetAllModesPaiementQuery request, CancellationToken cancellationToken)
    {
        var modes = await _unitOfWork.ModesPayement.GetAllAsync();
        return _mapper.Map<IEnumerable<ModePaiementDto>>(modes.OrderBy(m => m.LibelleMode));
    }
}
