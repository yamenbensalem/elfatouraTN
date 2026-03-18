using GestCom.Application.Features.Ventes.Commandes.DTOs;
using MediatR;
using System.Collections.Generic;
using System;

namespace GestCom.Application.Features.Ventes.Commandes.Commands.UpdateCommandeVente;

public class UpdateCommandeVenteCommand : IRequest<CommandeVenteDto>
{
    public string NumeroCommande { get; set; } = string.Empty;
    public DateTime DateCommande { get; set; }
    public DateTime? DateLivraisonPrevue { get; set; }
    public string CodeClient { get; set; } = string.Empty;
    public string? AdresseLivraison { get; set; }
    
    public decimal TauxRemise { get; set; }
    
    public string? CodeDevise { get; set; }
    public decimal TauxChange { get; set; } = 1;
    public string? Observations { get; set; }
    public string? Statut { get; set; }
    
    public List<UpdateLigneCommandeVenteDto> Lignes { get; set; } = new();
}

public class UpdateLigneCommandeVenteDto
{
    public string CodeProduit { get; set; } = string.Empty;
    public decimal Quantite { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal TauxRemise { get; set; }
    public decimal TauxTVA { get; set; }
}
