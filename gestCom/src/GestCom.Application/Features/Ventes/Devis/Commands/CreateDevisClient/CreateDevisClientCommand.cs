using GestCom.Application.Features.Ventes.Devis.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.Devis.Commands.CreateDevisClient;

public class CreateDevisClientCommand : IRequest<DevisClientDto>
{
    public DateTime DateDevis { get; set; } = DateTime.Now;
    public DateTime? DateValidite { get; set; }
    public string CodeClient { get; set; } = string.Empty;
    
    public decimal TauxRemise { get; set; }
    public decimal Timbre { get; set; }
    
    public string? CodeDevise { get; set; }
    public decimal TauxChange { get; set; } = 1;
    public string? Observations { get; set; }
    
    public List<CreateLigneDevisClientDto> Lignes { get; set; } = new();
}
