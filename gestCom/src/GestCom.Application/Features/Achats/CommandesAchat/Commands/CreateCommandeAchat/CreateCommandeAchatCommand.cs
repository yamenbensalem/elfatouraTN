using GestCom.Application.Features.Achats.CommandesAchat.DTOs;
using MediatR;

namespace GestCom.Application.Features.Achats.CommandesAchat.Commands.CreateCommandeAchat;

public class CreateCommandeAchatCommand : IRequest<CommandeAchatDto>
{
    public string CodeEntreprise { get; set; } = string.Empty;
    public DateTime DateCommande { get; set; } = DateTime.Now;
    public DateTime? DateLivraison { get; set; }
    public string CodeFournisseur { get; set; } = string.Empty;
    public decimal Remise { get; set; }
    public string? Notes { get; set; }
    
    public List<CreateLigneCommandeAchatDto> Lignes { get; set; } = new();
}
