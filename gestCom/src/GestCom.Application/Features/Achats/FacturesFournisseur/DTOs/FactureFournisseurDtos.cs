using System;
using System.Collections.Generic;

namespace GestCom.Application.Features.Achats.FacturesFournisseur.DTOs;

/// <summary>
/// DTO complet pour une facture fournisseur
/// </summary>
public class FactureFournisseurDto
{
    public string NumeroFacture { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public DateTime DateFacture { get; set; }
    public DateTime? DateEcheance { get; set; }
    
    // Fournisseur
    public string CodeFournisseur { get; set; } = string.Empty;
    public string? NomFournisseur { get; set; }
    public string? AdresseFournisseur { get; set; }
    public string? MatriculeFiscalFournisseur { get; set; }
    
    // Montants
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }
    public decimal MontantFODEC { get; set; }
    public decimal Timbre { get; set; }
    public decimal Remise { get; set; }
    public decimal TauxRemise { get; set; }
    
    // Retenue à la source
    public decimal TauxRAS { get; set; }
    public decimal MontantRAS { get; set; }
    public decimal NetAPayer { get; set; }
    
    // Paiement
    public decimal MontantRegle { get; set; }
    public decimal Reste => NetAPayer - MontantRegle;
    public bool EstPayee => Reste <= 0;
    
    // Statut
    public string? Statut { get; set; }
    public string? Observations { get; set; }
    
    // Documents liés
    public string? NumeroBonReception { get; set; }
    public string? NumeroCommande { get; set; }
    
    // Devise
    public string? CodeDevise { get; set; }
    public decimal TauxChange { get; set; }
    
    // Lignes
    public List<LigneFactureFournisseurDto> Lignes { get; set; } = new();
    
    // Audit
    public DateTime DateCreation { get; set; }
    public DateTime? DateModification { get; set; }
}

/// <summary>
/// DTO pour une ligne de facture fournisseur
/// </summary>
public class LigneFactureFournisseurDto
{
    public int NumeroLigne { get; set; }
    public string CodeProduit { get; set; } = string.Empty;
    public string? DesignationProduit { get; set; }
    public decimal Quantite { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal TauxTVA { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal TauxRemise { get; set; }
    public decimal MontantRemise { get; set; }
    public decimal MontantHT { get; set; }
    public decimal MontantTTC { get; set; }
}

/// <summary>
/// DTO simplifié pour les listes de factures
/// </summary>
public class FactureFournisseurListDto
{
    public string NumeroFacture { get; set; } = string.Empty;
    public DateTime DateFacture { get; set; }
    public string CodeFournisseur { get; set; } = string.Empty;
    public string? NomFournisseur { get; set; }
    public decimal MontantTTC { get; set; }
    public decimal NetAPayer { get; set; }
    public decimal MontantRegle { get; set; }
    public decimal Reste => NetAPayer - MontantRegle;
    public bool EstPayee => Reste <= 0;
    public string? Statut { get; set; }
    public int NombreLignes { get; set; }
}

/// <summary>
/// DTO pour la création d'une facture fournisseur
/// </summary>
public class CreateFactureFournisseurDto
{
    public DateTime DateFacture { get; set; } = DateTime.Now;
    public DateTime? DateEcheance { get; set; }
    public string CodeFournisseur { get; set; } = string.Empty;
    
    public decimal TauxRemise { get; set; }
    public decimal Timbre { get; set; }
    public decimal TauxRAS { get; set; }
    
    public string? CodeDevise { get; set; }
    public decimal TauxChange { get; set; } = 1;
    public string? CodeModePaiement { get; set; }
    
    public string? NumeroBonReception { get; set; }
    public string? NumeroCommande { get; set; }
    public string? Observations { get; set; }
    
    public List<CreateLigneFactureFournisseurDto> Lignes { get; set; } = new();
}

/// <summary>
/// DTO pour la création d'une ligne de facture
/// </summary>
public class CreateLigneFactureFournisseurDto
{
    public string CodeProduit { get; set; } = string.Empty;
    public decimal Quantite { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal TauxTVA { get; set; }
    public decimal TauxRemise { get; set; }
    public decimal TauxFODEC { get; set; }
}
