using AutoMapper;
using GestCom.Application.Features.Ventes.Commandes.DTOs;
using GestCom.Domain.Entities;

namespace GestCom.Application.Features.Ventes.Commandes.Mappings;

public class CommandeVenteMappingProfile : Profile
{
    public CommandeVenteMappingProfile()
    {
        // CommandeVente -> CommandeVenteDto
        CreateMap<CommandeVente, CommandeVenteDto>()
            .ForMember(dest => dest.NomClient, 
                opt => opt.MapFrom(src => src.Client != null ? src.Client.RaisonSociale : null))
            .ForMember(dest => dest.AdresseClient, 
                opt => opt.MapFrom(src => src.Client != null ? src.Client.Adresse : null))
            .ForMember(dest => dest.Lignes, 
                opt => opt.MapFrom(src => src.Lignes));

        // CommandeVente -> CommandeVenteListDto
        CreateMap<CommandeVente, CommandeVenteListDto>()
            .ForMember(dest => dest.NomClient, 
                opt => opt.MapFrom(src => src.Client != null ? src.Client.RaisonSociale : null))
            .ForMember(dest => dest.EstLivree, 
                opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.NumeroBonLivraison)))
            .ForMember(dest => dest.NombreLignes, 
                opt => opt.MapFrom(src => src.Lignes != null ? src.Lignes.Count : 0));

        // LigneCommandeVente -> LigneCommandeVenteDto
        CreateMap<LigneCommandeVente, LigneCommandeVenteDto>()
            .ForMember(dest => dest.DesignationProduit, 
                opt => opt.MapFrom(src => src.Produit != null ? src.Produit.Designation : null));

        // CreateCommandeVenteDto -> CommandeVente
        CreateMap<CreateCommandeVenteDto, CommandeVente>()
            .ForMember(dest => dest.NumeroCommande, opt => opt.Ignore())
            .ForMember(dest => dest.Lignes, opt => opt.Ignore());

        // CreateLigneCommandeVenteDto -> LigneCommandeVente
        CreateMap<CreateLigneCommandeVenteDto, LigneCommandeVente>()
            .ForMember(dest => dest.NumeroLigne, opt => opt.Ignore());
    }
}
