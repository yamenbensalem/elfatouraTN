using AutoMapper;
using GestCom.Domain.Entities;
using GestCom.Application.Features.Ventes.Clients.DTOs;

namespace GestCom.Application.Features.Ventes.Clients.Mappings;

/// <summary>
/// Profil AutoMapper pour Client
/// </summary>
public class ClientMappingProfile : Profile
{
    public ClientMappingProfile()
    {
        // Entity → DTO
        CreateMap<Client, ClientDto>()
            .ForMember(dest => dest.DeviseNom, opt => opt.MapFrom(src => src.Devise != null ? src.Devise.Nom : null))
            .ForMember(dest => dest.TotalCreances, opt => opt.Ignore()); // Calculé séparément

        CreateMap<Client, ClientListDto>()
            .ForMember(dest => dest.TotalCreances, opt => opt.Ignore());

        // DTO → Entity
        CreateMap<CreateClientDto, Client>();
        CreateMap<UpdateClientDto, Client>();
    }
}
