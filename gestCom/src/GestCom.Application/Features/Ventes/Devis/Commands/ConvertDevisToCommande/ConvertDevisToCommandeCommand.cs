using GestCom.Application.Features.Ventes.Commandes.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.Devis.Commands.ConvertDevisToCommande;

/// <summary>
/// Command pour convertir un devis en commande
/// </summary>
public class ConvertDevisToCommandeCommand : IRequest<CommandeVenteDto>
{
    public string NumeroDevis { get; set; } = string.Empty;
    public DateTime? DateCommande { get; set; }
    public string? Observation { get; set; }
}
