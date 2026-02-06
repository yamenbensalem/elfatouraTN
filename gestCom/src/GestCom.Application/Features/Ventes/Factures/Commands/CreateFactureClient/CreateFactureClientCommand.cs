using GestCom.Application.Features.Ventes.Factures.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.Factures.Commands.CreateFactureClient;

public class CreateFactureClientCommand : IRequest<FactureClientDto>
{
    public DateTime DateFacture { get; set; } = DateTime.Now;
    public DateTime? DateEcheance { get; set; }
    public string CodeClient { get; set; } = string.Empty;
    
    public decimal TauxRemise { get; set; }
    public decimal Timbre { get; set; }
    public decimal TauxRAS { get; set; }
    
    public string? CodeDevise { get; set; }
    public decimal TauxChange { get; set; } = 1;
    public string? CodeModePaiement { get; set; }
    
    public string? NumeroBonLivraison { get; set; }
    public string? NumeroCommande { get; set; }
    public string? Observations { get; set; }
    
    public List<CreateLigneFactureClientDto> Lignes { get; set; } = new();
}
