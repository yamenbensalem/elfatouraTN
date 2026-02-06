using AutoMapper;
using GestCom.Application.Features.Achats.BonsReception.DTOs;
using GestCom.Domain.Entities;

namespace GestCom.Application.Features.Achats.BonsReception.Mappings;

public class BonReceptionMappingProfile : Profile
{
    public BonReceptionMappingProfile()
    {
        CreateMap<BonReception, BonReceptionDto>()
            .ForMember(dest => dest.NomFournisseur, 
                opt => opt.MapFrom(src => src.Fournisseur != null ? src.Fournisseur.Nom : null))
            .ForMember(dest => dest.AdresseFournisseur, 
                opt => opt.MapFrom(src => src.Fournisseur != null ? src.Fournisseur.Adresse : null))
            .ForMember(dest => dest.Lignes, 
                opt => opt.MapFrom(src => src.Lignes));

        CreateMap<BonReception, BonReceptionListDto>()
            .ForMember(dest => dest.NomFournisseur, 
                opt => opt.MapFrom(src => src.Fournisseur != null ? src.Fournisseur.Nom : null))
            .ForMember(dest => dest.EstFacture, 
                opt => opt.MapFrom(src => src.Statut == "Facturé"))
            .ForMember(dest => dest.NombreLignes, 
                opt => opt.MapFrom(src => src.Lignes != null ? src.Lignes.Count : 0));

        CreateMap<LigneBonReception, LigneBonReceptionDto>()
            .ForMember(dest => dest.DesignationProduit, 
                opt => opt.MapFrom(src => src.Produit != null ? src.Produit.Designation : null));

        CreateMap<CreateBonReceptionDto, BonReception>()
            .ForMember(dest => dest.NumeroBon, opt => opt.Ignore())
            .ForMember(dest => dest.Lignes, opt => opt.Ignore());

        CreateMap<CreateLigneBonReceptionDto, LigneBonReception>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
