using GestCom.Application.Features.Achats.CommandesAchat.DTOs;
using MediatR;
using System.Collections.Generic;
using System;

namespace GestCom.Application.Features.Achats.CommandesAchat.Commands.UpdateCommandeAchat;

public class UpdateCommandeAchatCommand : IRequest<CommandeAchatDto>
{
    public string NumeroCommande { get; set; } = string.Empty;
    public DateTime DateCommande { get; set; }
    public DateTime? DateLivraisonPrevue { get; set; }
    public string CodeFournisseur { get; set; } = string.Empty;
    public string? Observation { get; set; }
    public string? CodeDevise { get; set; }
    public decimal TauxChange { get; set; } = 1;
    public string? Statut { get; set; }
    public decimal Remise { get; set; }

    public List<UpdateLigneCommandeAchatDto> Lignes { get; set; } = new();
}

public class UpdateLigneCommandeAchatDto
{
    public string CodeProduit { get; set; } = string.Empty;
    public decimal Quantite { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal TauxRemise { get; set; }
    public decimal TauxTVA { get; set; }
}