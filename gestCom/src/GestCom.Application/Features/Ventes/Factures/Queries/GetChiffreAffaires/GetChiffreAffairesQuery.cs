using GestCom.Application.Features.Ventes.Factures.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.Factures.Queries.GetChiffreAffaires;

public class GetChiffreAffairesQuery : IRequest<ChiffreAffairesDto>
{
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public string? CodeClient { get; set; }
    public bool IncludeParMois { get; set; } = true;
}
