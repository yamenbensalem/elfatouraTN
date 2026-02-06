using AutoMapper;
using GestCom.Application.Features.Achats.FacturesFournisseur.DTOs;
using GestCom.Domain.Entities;

namespace GestCom.Application.Features.Achats.FacturesFournisseur.Mappings;

public class FactureFournisseurMappingProfile : Profile
{
    public FactureFournisseurMappingProfile()
    {
        CreateMap<FactureFournisseur, FactureFournisseurDto>()
            .ForMember(dest => dest.NomFournisseur, 
                opt => opt.MapFrom(src => src.Fournisseur != null ? src.Fournisseur.Nom : null))
            .ForMember(dest => dest.AdresseFournisseur, 
                opt => opt.MapFrom(src => src.Fournisseur != null ? src.Fournisseur.Adresse : null))
            .ForMember(dest => dest.MatriculeFiscalFournisseur, 
                opt => opt.MapFrom(src => src.Fournisseur != null ? src.Fournisseur.MatriculeFiscale : null))
            .ForMember(dest => dest.Lignes, 
                opt => opt.MapFrom(src => src.Lignes));

        CreateMap<FactureFournisseur, FactureFournisseurListDto>()
            .ForMember(dest => dest.NomFournisseur, 
                opt => opt.MapFrom(src => src.Fournisseur != null ? src.Fournisseur.Nom : null))
            .ForMember(dest => dest.NombreLignes, 
                opt => opt.MapFrom(src => src.Lignes != null ? src.Lignes.Count : 0));

        CreateMap<LigneFactureFournisseur, LigneFactureFournisseurDto>()
            .ForMember(dest => dest.DesignationProduit, 
                opt => opt.MapFrom(src => src.Produit != null ? src.Produit.Designation : null));

        CreateMap<CreateFactureFournisseurDto, FactureFournisseur>()
            .ForMember(dest => dest.NumeroFacture, opt => opt.Ignore())
            .ForMember(dest => dest.Lignes, opt => opt.Ignore());

        CreateMap<CreateLigneFactureFournisseurDto, LigneFactureFournisseur>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
