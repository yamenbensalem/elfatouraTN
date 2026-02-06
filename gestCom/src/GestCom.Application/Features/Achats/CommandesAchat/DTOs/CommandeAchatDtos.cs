using System;
using System.Collections.Generic;

namespace GestCom.Application.Features.Achats.CommandesAchat.DTOs;

/// <summary>
/// DTO complet pour une commande d'achat
/// </summary>
public class CommandeAchatDto
{
    public string NumeroCommande { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public DateTime DateCommande { get; set; }
    public DateTime? DateLivraisonPrevue { get; set; }
    
    // Fournisseur
    public string CodeFournisseur { get; set; } = string.Empty;
    public string? NomFournisseur { get; set; }
    public string? AdresseFournisseur { get; set; }
    
    // Montants
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }
    public decimal Remise { get; set; }
    public decimal TauxRemise { get; set; }
    
    // Statut
    public string? Statut { get; set; }
    public string? Observations { get; set; }
    
    // Documents liés
    public string? NumeroDemandePrix { get; set; }
    public string? NumeroBonReception { get; set; }
    public bool EstRecu => !string.IsNullOrEmpty(NumeroBonReception);
    
    // Devise
    public string? CodeDevise { get; set; }
    public decimal TauxChange { get; set; }
    
    // Lignes
    public List<LigneCommandeAchatDto> Lignes { get; set; } = new();
    
    // Audit
    public DateTime DateCreation { get; set; }
    public DateTime? DateModification { get; set; }
}

/// <summary>
/// DTO pour une ligne de commande d'achat
/// </summary>
public class LigneCommandeAchatDto
{
    public int NumeroLigne { get; set; }
    public string CodeProduit { get; set; } = string.Empty;
    public string? DesignationProduit { get; set; }
    public decimal Quantite { get; set; }
    public decimal QuantiteRecue { get; set; }
    public decimal QuantiteRestante => Quantite - QuantiteRecue;
    public decimal PrixUnitaireHT { get; set; }
    public decimal TauxTVA { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantHT { get; set; }
    public decimal MontantTTC { get; set; }
}

/// <summary>
/// DTO simplifié pour les listes de commandes
/// </summary>
public class CommandeAchatListDto
{
    public string NumeroCommande { get; set; } = string.Empty;
    public DateTime DateCommande { get; set; }
    public DateTime? DateLivraisonPrevue { get; set; }
    public string CodeFournisseur { get; set; } = string.Empty;
    public string? NomFournisseur { get; set; }
    public decimal MontantTTC { get; set; }
    public string? Statut { get; set; }
    public bool EstRecu { get; set; }
    public int NombreLignes { get; set; }
}

/// <summary>
/// DTO pour la création d'une commande d'achat
/// </summary>
public class CreateCommandeAchatDto
{
    public DateTime DateCommande { get; set; } = DateTime.Now;
    public DateTime? DateLivraisonPrevue { get; set; }
    public string CodeFournisseur { get; set; } = string.Empty;
    
    public decimal TauxRemise { get; set; }
    
    public string? CodeDevise { get; set; }
    public decimal TauxChange { get; set; } = 1;
    public string? NumeroDemandePrix { get; set; }
    public string? Observations { get; set; }
    
    public List<CreateLigneCommandeAchatDto> Lignes { get; set; } = new();
}

/// <summary>
/// DTO pour la création d'une ligne de commande
/// </summary>
public class CreateLigneCommandeAchatDto
{
    public string CodeProduit { get; set; } = string.Empty;
    public decimal Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal TauxTVA { get; set; }
    public decimal Remise { get; set; }
}
