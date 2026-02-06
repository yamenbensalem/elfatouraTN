using AutoMapper;
using GestCom.Application.Features.Ventes.Factures.DTOs;
using GestCom.Domain.Entities;

namespace GestCom.Application.Features.Ventes.Factures.Mappings;

public class FactureClientMappingProfile : Profile
{
    public FactureClientMappingProfile()
    {
        // FactureClient -> FactureClientDto
        CreateMap<FactureClient, FactureClientDto>()
            .ForMember(dest => dest.NomClient, 
                opt => opt.MapFrom(src => src.Client != null ? src.Client.Nom : null))
            .ForMember(dest => dest.AdresseClient, 
                opt => opt.MapFrom(src => src.Client != null ? src.Client.Adresse : null))
            .ForMember(dest => dest.MatriculeFiscalClient, 
                opt => opt.MapFrom(src => src.Client != null ? src.Client.MatriculeFiscale : null))
            .ForMember(dest => dest.SymboleDevise, 
                opt => opt.MapFrom(src => src.Client != null && src.Client.Devise != null ? src.Client.Devise.Symbole : null))
            .ForMember(dest => dest.LibelleModePaiement, 
                opt => opt.MapFrom(src => src.ModePayementNavigation != null ? src.ModePayementNavigation.Designation : null))
            .ForMember(dest => dest.Lignes, 
                opt => opt.MapFrom(src => src.Lignes));

        // FactureClient -> FactureClientListDto
        CreateMap<FactureClient, FactureClientListDto>()
            .ForMember(dest => dest.NomClient, 
                opt => opt.MapFrom(src => src.Client != null ? src.Client.Nom : null))
            .ForMember(dest => dest.NombreLignes, 
                opt => opt.MapFrom(src => src.Lignes != null ? src.Lignes.Count : 0));

        // LigneFactureClient -> LigneFactureClientDto
        CreateMap<LigneFactureClient, LigneFactureClientDto>()
            .ForMember(dest => dest.DesignationProduit, 
                opt => opt.MapFrom(src => src.Produit != null ? src.Produit.Designation : null));

        // CreateFactureClientDto -> FactureClient
        CreateMap<CreateFactureClientDto, FactureClient>()
            .ForMember(dest => dest.NumeroFacture, opt => opt.Ignore()) // Généré automatiquement
            .ForMember(dest => dest.Lignes, opt => opt.Ignore()); // Géré séparément

        // CreateLigneFactureClientDto -> LigneFactureClient
        CreateMap<CreateLigneFactureClientDto, LigneFactureClient>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()); // Généré automatiquement
    }
}
