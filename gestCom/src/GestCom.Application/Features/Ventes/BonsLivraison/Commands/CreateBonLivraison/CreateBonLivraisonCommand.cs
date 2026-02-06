using GestCom.Application.Features.Ventes.BonsLivraison.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.BonsLivraison.Commands.CreateBonLivraison;

public class CreateBonLivraisonCommand : IRequest<BonLivraisonDto>
{
    public DateTime DateBonLivraison { get; set; } = DateTime.Now;
    public string CodeClient { get; set; } = string.Empty;
    public string? AdresseLivraison { get; set; }
    
    public string? NumeroCommande { get; set; }
    public string? Observations { get; set; }
    
    public List<CreateLigneBonLivraisonDto> Lignes { get; set; } = new();
}
