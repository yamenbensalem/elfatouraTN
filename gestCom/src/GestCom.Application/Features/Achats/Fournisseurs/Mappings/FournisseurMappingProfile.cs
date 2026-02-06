using AutoMapper;
using GestCom.Application.Features.Achats.Fournisseurs.DTOs;
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
    }
}
