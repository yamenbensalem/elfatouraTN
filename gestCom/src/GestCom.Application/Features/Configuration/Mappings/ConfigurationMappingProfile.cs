using AutoMapper;
using GestCom.Application.Features.Configuration.Categories.DTOs;
using GestCom.Application.Features.Configuration.Devises.DTOs;
using GestCom.Application.Features.Configuration.Entreprise.DTOs;
using GestCom.Application.Features.Configuration.Magasins.DTOs;
using GestCom.Application.Features.Configuration.ModesPaiement.DTOs;
using GestCom.Application.Features.Configuration.TVA.DTOs;
using GestCom.Application.Features.Configuration.Unites.DTOs;
using GestCom.Domain.Entities;
using EntrepriseEntity = GestCom.Domain.Entities.Entreprise;

namespace GestCom.Application.Features.Configuration.Mappings;

/// <summary>
/// Profil de mapping pour toutes les entités de configuration
/// </summary>
public class ConfigurationMappingProfile : Profile
{
    public ConfigurationMappingProfile()
    {
        // Entreprise
        CreateMap<EntrepriseEntity, EntrepriseDto>()
            .ForMember(dest => dest.SymboleDevise, 
                opt => opt.MapFrom(src => src.Devise != null ? src.Devise.Symbole : null));
        CreateMap<UpdateEntrepriseDto, EntrepriseEntity>();

        // Devise
        CreateMap<Devise, DeviseDto>();
        CreateMap<CreateDeviseDto, Devise>();
        CreateMap<UpdateDeviseDto, Devise>();

        // TVA
        CreateMap<TvaProduit, TvaProduitDto>();
        CreateMap<CreateTvaProduitDto, TvaProduit>();
        CreateMap<UpdateTvaProduitDto, TvaProduit>();

        // Catégorie Produit
        CreateMap<CategorieProduit, CategorieProduitDto>()
            .ForMember(dest => dest.NombreProduits, 
                opt => opt.MapFrom(src => src.Produits != null ? src.Produits.Count : 0));
        CreateMap<CategorieProduit, CategorieProduitListDto>()
            .ForMember(dest => dest.NombreProduits, 
                opt => opt.MapFrom(src => src.Produits != null ? src.Produits.Count : 0));
        CreateMap<CreateCategorieProduitDto, CategorieProduit>();
        CreateMap<UpdateCategorieProduitDto, CategorieProduit>();

        // Unité Produit
        CreateMap<UniteProduit, UniteProduitDto>();
        CreateMap<CreateUniteProduitDto, UniteProduit>();
        CreateMap<UpdateUniteProduitDto, UniteProduit>();

        // Magasin Produit
        CreateMap<MagasinProduit, MagasinProduitDto>()
            .ForMember(dest => dest.NombreProduits, 
                opt => opt.MapFrom(src => src.Produits != null ? src.Produits.Count : 0));
        CreateMap<MagasinProduit, MagasinProduitListDto>()
            .ForMember(dest => dest.NombreProduits, 
                opt => opt.MapFrom(src => src.Produits != null ? src.Produits.Count : 0));
        CreateMap<CreateMagasinProduitDto, MagasinProduit>();
        CreateMap<UpdateMagasinProduitDto, MagasinProduit>();

        // Mode Paiement
        CreateMap<ModePayement, ModePaiementDto>();
        CreateMap<CreateModePaiementDto, ModePayement>();
        CreateMap<UpdateModePaiementDto, ModePayement>();
    }
}
