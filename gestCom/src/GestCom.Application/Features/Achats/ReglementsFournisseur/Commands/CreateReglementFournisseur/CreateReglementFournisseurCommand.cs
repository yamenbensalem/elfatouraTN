using GestCom.Application.Features.Achats.ReglementsFournisseur.DTOs;
using MediatR;

namespace GestCom.Application.Features.Achats.ReglementsFournisseur.Commands.CreateReglementFournisseur;

/// <summary>
/// Command pour créer un règlement fournisseur
/// </summary>
public class CreateReglementFournisseurCommand : IRequest<ReglementFournisseurDto>
{
    public string CodeEntreprise { get; set; } = string.Empty;
    public DateTime DateReglement { get; set; }
    public string NumeroFacture { get; set; } = string.Empty;
    public decimal Montant { get; set; }
    public string? ModePayement { get; set; }
    public string? NumeroTransaction { get; set; }
    public string? Notes { get; set; }
}
