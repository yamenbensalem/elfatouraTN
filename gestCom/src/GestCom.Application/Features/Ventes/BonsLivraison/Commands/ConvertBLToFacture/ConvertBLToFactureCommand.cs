using GestCom.Application.Features.Ventes.Factures.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.BonsLivraison.Commands.ConvertBLToFacture;

/// <summary>
/// Command pour convertir un ou plusieurs bons de livraison en facture
/// </summary>
public class ConvertBLToFactureCommand : IRequest<FactureClientDto>
{
    public List<string> NumerosBonLivraison { get; set; } = new();
    public DateTime DateFacture { get; set; }
    public DateTime DateEcheance { get; set; }
    public decimal TauxRemiseGlobale { get; set; }
    public string? Observation { get; set; }
}
