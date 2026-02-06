using GestCom.Application.Features.Ventes.Reglements.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.Reglements.Commands.CreateReglementFacture;

public class CreateReglementFactureCommand : IRequest<ReglementFactureDto>
{
    public string CodeEntreprise { get; set; } = string.Empty;
    public DateTime DateReglement { get; set; } = DateTime.Now;
    public string NumeroFacture { get; set; } = string.Empty;
    public decimal Montant { get; set; }
    public string? ModePayement { get; set; }
    public string? NumeroTransaction { get; set; }
    public string? Notes { get; set; }
}
