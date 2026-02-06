using AutoMapper;
using GestCom.Application.Features.Ventes.Devis.DTOs;
using GestCom.Domain.Entities;

namespace GestCom.Application.Features.Ventes.Devis.Mappings;

public class DevisClientMappingProfile : Profile
{
    public DevisClientMappingProfile()
    {
        // DevisClient -> DevisClientDto
        CreateMap<DevisClient, DevisClientDto>()
            .ForMember(dest => dest.NomClient, 
                opt => opt.MapFrom(src => src.Client != null ? src.Client.Nom : null))
            .ForMember(dest => dest.AdresseClient, 
                opt => opt.MapFrom(src => src.Client != null ? src.Client.Adresse : null))
            .ForMember(dest => dest.Lignes, 
                opt => opt.MapFrom(src => src.Lignes));

        // DevisClient -> DevisClientListDto
        CreateMap<DevisClient, DevisClientListDto>()
            .ForMember(dest => dest.NomClient, 
                opt => opt.MapFrom(src => src.Client != null ? src.Client.Nom : null))
            .ForMember(dest => dest.EstConverti, 
                opt => opt.MapFrom(src => src.Statut == "Accepté"))
            .ForMember(dest => dest.NombreLignes, 
                opt => opt.MapFrom(src => src.Lignes != null ? src.Lignes.Count : 0));

        // LigneDevisClient -> LigneDevisClientDto
        CreateMap<LigneDevisClient, LigneDevisClientDto>()
            .ForMember(dest => dest.DesignationProduit, 
                opt => opt.MapFrom(src => src.Produit != null ? src.Produit.Designation : null));

        // CreateDevisClientDto -> DevisClient
        CreateMap<CreateDevisClientDto, DevisClient>()
            .ForMember(dest => dest.NumeroDevis, opt => opt.Ignore())
            .ForMember(dest => dest.Lignes, opt => opt.Ignore());

        // CreateLigneDevisClientDto -> LigneDevisClient
        CreateMap<CreateLigneDevisClientDto, LigneDevisClient>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
