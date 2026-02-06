using AutoMapper;
using GestCom.Application.Features.Configuration.DTOs;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Configuration.Entreprise.Queries.GetEntreprise;

public class GetEntrepriseQueryHandler : IRequestHandler<GetEntrepriseQuery, EntrepriseDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetEntrepriseQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<EntrepriseDto?> Handle(GetEntrepriseQuery request, CancellationToken cancellationToken)
    {
        var entreprises = await _unitOfWork.Entreprises.GetAllAsync();
        
        Domain.Entities.Entreprise? entreprise;
        
        if (!string.IsNullOrEmpty(request.CodeEntreprise))
        {
            entreprise = entreprises.FirstOrDefault(e => e.CodeEntreprise == request.CodeEntreprise);
        }
        else
        {
            // En mode single-tenant, récupérer la première (et unique) entreprise
            entreprise = entreprises.FirstOrDefault();
        }

        return entreprise != null ? _mapper.Map<EntrepriseDto>(entreprise) : null;
    }
}
