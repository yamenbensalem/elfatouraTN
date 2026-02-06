using System;
using System.Collections.Generic;

namespace GestCom.Application.Features.Achats.BonsReception.DTOs;

/// <summary>
/// DTO complet pour un bon de réception
/// </summary>
public class BonReceptionDto
{
    public string NumeroBonReception { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public DateTime DateBonReception { get; set; }
    
    // Fournisseur
    public string CodeFournisseur { get; set; } = string.Empty;
    public string? NomFournisseur { get; set; }
    public string? AdresseFournisseur { get; set; }
    
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
    public List<LigneBonReceptionDto> Lignes { get; set; } = new();
    
    // Audit
    public DateTime DateCreation { get; set; }
    public DateTime? DateModification { get; set; }
}

/// <summary>
/// DTO pour une ligne de bon de réception
/// </summary>
public class LigneBonReceptionDto
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
/// DTO simplifié pour les listes
/// </summary>
public class BonReceptionListDto
{
    public string NumeroBonReception { get; set; } = string.Empty;
    public DateTime DateBonReception { get; set; }
    public string CodeFournisseur { get; set; } = string.Empty;
    public string? NomFournisseur { get; set; }
    public decimal MontantTTC { get; set; }
    public string? Statut { get; set; }
    public bool EstFacture { get; set; }
    public string? NumeroCommande { get; set; }
    public int NombreLignes { get; set; }
}

/// <summary>
/// DTO pour la création d'un bon de réception
/// </summary>
public class CreateBonReceptionDto
{
    public DateTime DateBonReception { get; set; } = DateTime.Now;
    public string CodeFournisseur { get; set; } = string.Empty;
    
    public string? NumeroCommande { get; set; }
    public string? Observations { get; set; }
    
    public List<CreateLigneBonReceptionDto> Lignes { get; set; } = new();
}

/// <summary>
/// DTO pour la création d'une ligne de BR
/// </summary>
public class CreateLigneBonReceptionDto
{
    public string CodeProduit { get; set; } = string.Empty;
    public decimal Quantite { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal TauxTVA { get; set; }
}
