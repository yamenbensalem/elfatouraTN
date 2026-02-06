using AutoMapper;
using GestCom.Application.Features.Achats.Fournisseurs.DTOs;
using GestCom.Application.Features.Achats.Fournisseurs.Commands.CreateFournisseur;
using GestCom.Domain.Entities;

namespace GestCom.Application.Features.Achats.Fournisseurs.Mappings;

public class FournisseurMappingProfile : Profile
{
    public FournisseurMappingProfile()
    {
        CreateMap<Fournisseur, FournisseurDto>();
        CreateMap<Fournisseur, FournisseurListDto>();
        CreateMap<CreateFournisseurDto, Fournisseur>();
        CreateMap<UpdateFournisseurDto, Fournisseur>();

        // Command → Entity
        CreateMap<CreateFournisseurCommand, Fournisseur>()
            .ForMember(dest => dest.Nom, opt => opt.MapFrom(src => src.RaisonSociale))
            .ForMember(dest => dest.Tel, opt => opt.MapFrom(src => src.Telephone));
    }
}
