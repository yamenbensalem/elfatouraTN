using AutoMapper;
using GestCom.Application.Features.Achats.CommandesAchat.DTOs;
using GestCom.Domain.Entities;

namespace GestCom.Application.Features.Achats.CommandesAchat.Mappings;

public class CommandeAchatMappingProfile : Profile
{
    public CommandeAchatMappingProfile()
    {
        CreateMap<CommandeAchat, CommandeAchatDto>()
            .ForMember(dest => dest.NomFournisseur, 
                opt => opt.MapFrom(src => src.Fournisseur != null ? src.Fournisseur.Nom : null))
            .ForMember(dest => dest.AdresseFournisseur, 
                opt => opt.MapFrom(src => src.Fournisseur != null ? src.Fournisseur.Adresse : null))
            .ForMember(dest => dest.Lignes, 
                opt => opt.MapFrom(src => src.Lignes));

        CreateMap<CommandeAchat, CommandeAchatListDto>()
            .ForMember(dest => dest.NomFournisseur, 
                opt => opt.MapFrom(src => src.Fournisseur != null ? src.Fournisseur.Nom : null))
            .ForMember(dest => dest.EstRecu, 
                opt => opt.MapFrom(src => src.BonsReception != null && src.BonsReception.Any()))
            .ForMember(dest => dest.NombreLignes, 
                opt => opt.MapFrom(src => src.Lignes != null ? src.Lignes.Count : 0));

        CreateMap<LigneCommandeAchat, LigneCommandeAchatDto>()
            .ForMember(dest => dest.DesignationProduit, 
                opt => opt.MapFrom(src => src.Produit != null ? src.Produit.Designation : null));

        CreateMap<CreateCommandeAchatDto, CommandeAchat>()
            .ForMember(dest => dest.NumeroCommande, opt => opt.Ignore())
            .ForMember(dest => dest.Lignes, opt => opt.Ignore());

        CreateMap<CreateLigneCommandeAchatDto, LigneCommandeAchat>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
