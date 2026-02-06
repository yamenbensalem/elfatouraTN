using AutoMapper;
using GestCom.Application.Features.Configuration.DTOs;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Configuration.Entreprise.Commands.UpdateEntreprise;

public class UpdateEntrepriseCommandHandler : IRequestHandler<UpdateEntrepriseCommand, EntrepriseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateEntrepriseCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<EntrepriseDto> Handle(UpdateEntrepriseCommand request, CancellationToken cancellationToken)
    {
        var entreprises = await _unitOfWork.Entreprises.GetAllAsync();
        var entreprise = entreprises.FirstOrDefault(e => e.CodeEntreprise == request.CodeEntreprise);

        if (entreprise == null)
        {
            throw new InvalidOperationException($"Entreprise avec le code '{request.CodeEntreprise}' non trouvée.");
        }

        // Mettre à jour les propriétés
        entreprise.RaisonSociale = request.RaisonSociale;
        entreprise.MatriculeFiscal = request.MatriculeFiscal;
        entreprise.Adresse = request.Adresse;
        entreprise.CodePostal = request.CodePostal;
        entreprise.Ville = request.Ville;
        entreprise.Telephone = request.Telephone;
        entreprise.Fax = request.Fax;
        entreprise.Email = request.Email;
        entreprise.SiteWeb = request.SiteWeb;
        entreprise.RIB = request.RIB;
        entreprise.NomBanque = request.NomBanque;
        if (!string.IsNullOrEmpty(request.CodeDevise) && int.TryParse(request.CodeDevise, out var codeDevise))
        {
            entreprise.CodeDevise = codeDevise;
        }
        entreprise.Logo = request.Logo;

        await _unitOfWork.Entreprises.UpdateAsync(entreprise);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<EntrepriseDto>(entreprise);
    }
}
