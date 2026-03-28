using AutoMapper;
using GestCom.Application.Features.Ventes.Reglements.DTOs;
using GestCom.Domain.Entities;

namespace GestCom.Application.Features.Ventes.Reglements.Mappings;

public class ReglementFactureMappingProfile : Profile
{
    public ReglementFactureMappingProfile()
    {
        CreateMap<ReglementFacture, ReglementFactureDto>()
            .ForMember(dest => dest.NumeroReglement,
                opt => opt.MapFrom(src => $"REG-{src.DateReglement:yyyyMMdd}-{src.Id:D6}"))
            .ForMember(dest => dest.MontantFacture,
                opt => opt.MapFrom(src =>
                    src.FactureClient != null
                        ? (src.FactureClient.APayer > 0 ? src.FactureClient.APayer : src.FactureClient.NetAPayer)
                        : 0m))
            .ForMember(dest => dest.ResteARegler,
                opt => opt.MapFrom(src => src.FactureClient != null ? src.FactureClient.MontantRestant : 0m))
            .ForMember(dest => dest.NomClient,
                opt => opt.MapFrom(src =>
                    src.Client != null
                        ? src.Client.Nom
                        : src.FactureClient != null && src.FactureClient.Client != null
                            ? src.FactureClient.Client.Nom
                            : null))
            .ForMember(dest => dest.CodeModePaiement,
                opt => opt.MapFrom(src => src.ModePayement))
            .ForMember(dest => dest.LibelleModePaiement,
                opt => opt.MapFrom(src => src.ModePayement))
            .ForMember(dest => dest.Reference,
                opt => opt.MapFrom(src => src.NumeroTransaction))
            .ForMember(dest => dest.Observations,
                opt => opt.MapFrom(src => src.Notes))
            .ForMember(dest => dest.Banque,
                opt => opt.Ignore())
            .ForMember(dest => dest.DateEcheance,
                opt => opt.Ignore());

        CreateMap<ReglementFacture, ReglementFactureListDto>()
            .ForMember(dest => dest.NumeroReglement,
                opt => opt.MapFrom(src => $"REG-{src.DateReglement:yyyyMMdd}-{src.Id:D6}"))
            .ForMember(dest => dest.NomClient,
                opt => opt.MapFrom(src =>
                    src.Client != null
                        ? src.Client.Nom
                        : src.FactureClient != null && src.FactureClient.Client != null
                            ? src.FactureClient.Client.Nom
                            : null))
            .ForMember(dest => dest.LibelleModePaiement,
                opt => opt.MapFrom(src => src.ModePayement))
            .ForMember(dest => dest.Reference,
                opt => opt.MapFrom(src => src.NumeroTransaction));
    }
}