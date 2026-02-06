using AutoMapper;
using GestCom.Application.Features.Ventes.BonsLivraison.DTOs;
using GestCom.Domain.Entities;

namespace GestCom.Application.Features.Ventes.BonsLivraison.Mappings;

public class BonLivraisonMappingProfile : Profile
{
    public BonLivraisonMappingProfile()
    {
        CreateMap<BonLivraison, BonLivraisonDto>()
            .ForMember(dest => dest.NomClient, 
                opt => opt.MapFrom(src => src.Client != null ? src.Client.Nom : null))
            .ForMember(dest => dest.AdresseClient, 
                opt => opt.MapFrom(src => src.Client != null ? src.Client.Adresse : null))
            .ForMember(dest => dest.Lignes, 
                opt => opt.MapFrom(src => src.Lignes));

        CreateMap<BonLivraison, BonLivraisonListDto>()
            .ForMember(dest => dest.NomClient, 
                opt => opt.MapFrom(src => src.Client != null ? src.Client.Nom : null))
            .ForMember(dest => dest.EstFacture, 
                opt => opt.MapFrom(src => src.Facture))
            .ForMember(dest => dest.NombreLignes, 
                opt => opt.MapFrom(src => src.Lignes != null ? src.Lignes.Count : 0));

        CreateMap<LigneBonLivraison, LigneBonLivraisonDto>()
            .ForMember(dest => dest.DesignationProduit, 
                opt => opt.MapFrom(src => src.Produit != null ? src.Produit.Designation : null));

        CreateMap<CreateBonLivraisonDto, BonLivraison>()
            .ForMember(dest => dest.NumeroBon, opt => opt.Ignore())
            .ForMember(dest => dest.Lignes, opt => opt.Ignore());

        CreateMap<CreateLigneBonLivraisonDto, LigneBonLivraison>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
