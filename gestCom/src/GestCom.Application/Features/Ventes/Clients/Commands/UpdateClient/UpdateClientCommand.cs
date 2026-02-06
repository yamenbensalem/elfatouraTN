using GestCom.Application.Features.Ventes.Clients.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.Clients.Commands.UpdateClient;

/// <summary>
/// Commande pour mettre à jour un client existant
/// </summary>
public class UpdateClientCommand : IRequest<ClientDto>
{
    public string CodeClient { get; set; } = string.Empty;
    public string MatriculeFiscale { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string TypePersonne { get; set; } = "Personne Morale";
    public string? TypeEntreprise { get; set; }
    public string? RIB { get; set; }
    
    public string Adresse { get; set; } = string.Empty;
    public string? CodePostal { get; set; }
    public string Ville { get; set; } = string.Empty;
    public string Pays { get; set; } = "Tunisie";
    
    public string? Tel { get; set; }
    public string? TelMobile { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? SiteWeb { get; set; }
    
    public string Etat { get; set; } = "Actif";
    public decimal MaxCredit { get; set; }
    public int CodeDevise { get; set; } = 1;
    public string? Responsable { get; set; }
    public string? Note { get; set; }
}
