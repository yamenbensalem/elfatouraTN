using System;
using System.Collections.Generic;

namespace GestCom.Application.Features.Ventes.BonsLivraison.DTOs;

/// <summary>
/// DTO complet pour un bon de livraison
/// </summary>
public class BonLivraisonDto
{
    public string NumeroBonLivraison { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public DateTime DateBonLivraison { get; set; }
    
    // Client
    public string CodeClient { get; set; } = string.Empty;
    public string? NomClient { get; set; }
    public string? AdresseClient { get; set; }
    public string? AdresseLivraison { get; set; }
    
    // Montants
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }
    
    // Statut
    public string? Statut { get; set; }
    public string? Observations { get; set; }
    
    // Documents liés
    public string? NumeroCommande { get; set; }
    public string? NumeroFacture { get; set; }
    public bool EstFacture => !string.IsNullOrEmpty(NumeroFacture);
    
    // Lignes
    public List<LigneBonLivraisonDto> Lignes { get; set; } = new();
    
    // Audit
    public DateTime DateCreation { get; set; }
    public DateTime? DateModification { get; set; }
}

/// <summary>
/// DTO pour une ligne de bon de livraison
/// </summary>
public class LigneBonLivraisonDto
{
    public int NumeroLigne { get; set; }
    public string CodeProduit { get; set; } = string.Empty;
    public string? DesignationProduit { get; set; }
    public decimal Quantite { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal TauxTVA { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantHT { get; set; }
    public decimal MontantTTC { get; set; }
}

/// <summary>
/// DTO simplifié pour les listes de bons de livraison
/// </summary>
public class BonLivraisonListDto
{
    public string NumeroBonLivraison { get; set; } = string.Empty;
    public DateTime DateBonLivraison { get; set; }
    public string CodeClient { get; set; } = string.Empty;
    public string? NomClient { get; set; }
    public decimal MontantTTC { get; set; }
    public string? Statut { get; set; }
    public bool EstFacture { get; set; }
    public string? NumeroCommande { get; set; }
    public int NombreLignes { get; set; }
}

/// <summary>
/// DTO pour la création d'un bon de livraison
/// </summary>
public class CreateBonLivraisonDto
{
    public DateTime DateBonLivraison { get; set; } = DateTime.Now;
    public string CodeClient { get; set; } = string.Empty;
    public string? AdresseLivraison { get; set; }
    
    public string? NumeroCommande { get; set; }
    public string? Observations { get; set; }
    
    public List<CreateLigneBonLivraisonDto> Lignes { get; set; } = new();
}

/// <summary>
/// DTO pour la création d'une ligne de BL
/// </summary>
public class CreateLigneBonLivraisonDto
{
    public string CodeProduit { get; set; } = string.Empty;
    public decimal Quantite { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal TauxTVA { get; set; }
}

/// <summary>
/// DTO pour créer un BL depuis une commande
/// </summary>
public class CreateBonLivraisonFromCommandeDto
{
    public string NumeroCommande { get; set; } = string.Empty;
    public DateTime DateBonLivraison { get; set; } = DateTime.Now;
    public string? AdresseLivraison { get; set; }
    public string? Observations { get; set; }
    public List<LigneALivrerDto>? LignesALivrer { get; set; }
}

/// <summary>
/// DTO pour spécifier les quantités à livrer
/// </summary>
public class LigneALivrerDto
{
    public int NumeroLigne { get; set; }
    public decimal QuantiteALivrer { get; set; }
}
