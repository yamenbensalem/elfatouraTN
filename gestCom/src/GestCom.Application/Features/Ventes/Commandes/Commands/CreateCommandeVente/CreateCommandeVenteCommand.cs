using GestCom.Application.Features.Ventes.Commandes.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.Commandes.Commands.CreateCommandeVente;

public class CreateCommandeVenteCommand : IRequest<CommandeVenteDto>
{
    public DateTime DateCommande { get; set; } = DateTime.Now;
    public DateTime? DateLivraisonPrevue { get; set; }
    public string CodeClient { get; set; } = string.Empty;
    public string? AdresseLivraison { get; set; }
    
    public decimal TauxRemise { get; set; }
    
    public string? CodeDevise { get; set; }
    public decimal TauxChange { get; set; } = 1;
    public string? NumeroDevis { get; set; }
    public string? Observations { get; set; }
    
    public List<CreateLigneCommandeVenteDto> Lignes { get; set; } = new();
}
