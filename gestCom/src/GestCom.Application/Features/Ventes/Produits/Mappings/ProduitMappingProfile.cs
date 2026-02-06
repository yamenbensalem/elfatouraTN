using AutoMapper;
using GestCom.Application.Features.Ventes.Produits.DTOs;
using GestCom.Application.Features.Ventes.Produits.Commands.CreateProduit;
using GestCom.Domain.Entities;

namespace GestCom.Application.Features.Ventes.Produits.Mappings;

public class ProduitMappingProfile : Profile
{
    public ProduitMappingProfile()
    {
        // Entity -> DTO
        CreateMap<Produit, ProduitDto>()
            .ForMember(dest => dest.NomFournisseur, 
                opt => opt.MapFrom(src => src.Fournisseur != null ? src.Fournisseur.Nom : null))
            .ForMember(dest => dest.LibelleUnite, 
                opt => opt.MapFrom(src => src.UniteProduit != null ? src.UniteProduit.Designation : null))
            .ForMember(dest => dest.LibelleCategorie, 
                opt => opt.MapFrom(src => src.CategorieProduit != null ? src.CategorieProduit.Designation : null))
            .ForMember(dest => dest.LibelleMagasin, 
                opt => opt.MapFrom(src => src.MagasinProduit != null ? src.MagasinProduit.Libelle : null));

        CreateMap<Produit, ProduitListDto>()
            .ForMember(dest => dest.NomFournisseur, 
                opt => opt.MapFrom(src => src.Fournisseur != null ? src.Fournisseur.Nom : null))
            .ForMember(dest => dest.LibelleCategorie, 
                opt => opt.MapFrom(src => src.CategorieProduit != null ? src.CategorieProduit.Designation : null));

        CreateMap<Produit, SearchProduitDto>();

        // DTO -> Entity
        CreateMap<CreateProduitDto, Produit>();
        CreateMap<UpdateProduitDto, Produit>();

        // Command → Entity
        CreateMap<CreateProduitCommand, Produit>()
            .AfterMap((src, dest) =>
            {
                if (!string.IsNullOrEmpty(src.CodeUnite) && int.TryParse(src.CodeUnite, out var u))
                    dest.CodeUniteProduit = u;
                if (!string.IsNullOrEmpty(src.CodeCategorie) && int.TryParse(src.CodeCategorie, out var c))
                    dest.CodeCategorieProduit = c;
                if (!string.IsNullOrEmpty(src.CodeMagasin) && int.TryParse(src.CodeMagasin, out var m))
                    dest.CodeMagasinProduit = m;
                if (!string.IsNullOrEmpty(src.CodeTVA) && int.TryParse(src.CodeTVA, out var t))
                    dest.CodeTVAProduit = t;
            });
    }
}
