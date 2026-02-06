using System;
using System.Collections.Generic;

namespace GestCom.Application.Features.Ventes.Commandes.DTOs;

/// <summary>
/// DTO complet pour une commande de vente
/// </summary>
public class CommandeVenteDto
{
    public string NumeroCommande { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public DateTime DateCommande { get; set; }
    public DateTime? DateLivraisonPrevue { get; set; }
    
    // Client
    public string CodeClient { get; set; } = string.Empty;
    public string? NomClient { get; set; }
    public string? AdresseClient { get; set; }
    public string? AdresseLivraison { get; set; }
    
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
    public string? NumeroDevis { get; set; }
    public string? NumeroBonLivraison { get; set; }
    public bool EstLivree => !string.IsNullOrEmpty(NumeroBonLivraison);
    
    // Devise
    public string? CodeDevise { get; set; }
    public decimal TauxChange { get; set; }
    
    // Lignes
    public List<LigneCommandeVenteDto> Lignes { get; set; } = new();
    
    // Audit
    public DateTime DateCreation { get; set; }
    public DateTime? DateModification { get; set; }
}

/// <summary>
/// DTO pour une ligne de commande
/// </summary>
public class LigneCommandeVenteDto
{
    public int NumeroLigne { get; set; }
    public string CodeProduit { get; set; } = string.Empty;
    public string? DesignationProduit { get; set; }
    public decimal Quantite { get; set; }
    public decimal QuantiteLivree { get; set; }
    public decimal QuantiteRestante => Quantite - QuantiteLivree;
    public decimal PrixUnitaireHT { get; set; }
    public decimal TauxTVA { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal TauxRemise { get; set; }
    public decimal MontantRemise { get; set; }
    public decimal MontantHT { get; set; }
    public decimal MontantTTC { get; set; }
}

/// <summary>
/// DTO simplifié pour les listes de commandes
/// </summary>
public class CommandeVenteListDto
{
    public string NumeroCommande { get; set; } = string.Empty;
    public DateTime DateCommande { get; set; }
    public DateTime? DateLivraisonPrevue { get; set; }
    public string CodeClient { get; set; } = string.Empty;
    public string? NomClient { get; set; }
    public decimal MontantTTC { get; set; }
    public string? Statut { get; set; }
    public bool EstLivree { get; set; }
    public int NombreLignes { get; set; }
}

/// <summary>
/// DTO pour la création d'une commande
/// </summary>
public class CreateCommandeVenteDto
{
    public DateTime DateCommande { get; set; } = DateTime.Now;
    public DateTime? DateLivraisonPrevue { get; set; }
    public string CodeClient { get; set; } = string.Empty;
    public string? AdresseLivraison { get; set; }
    
    public decimal TauxRemise { get; set; }
    
    public string? CodeDevise { get; set; }
    public decimal TauxChange { get; set; } = 1;
    public string? NumeroDevis { get; set; }
    public string? Observations { get; set; }
    
    public List<CreateLigneCommandeVenteDto> Lignes { get; set; } = new();
}

/// <summary>
/// DTO pour la création d'une ligne de commande
/// </summary>
public class CreateLigneCommandeVenteDto
{
    public string CodeProduit { get; set; } = string.Empty;
    public decimal Quantite { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal TauxTVA { get; set; }
    public decimal TauxRemise { get; set; }
}
