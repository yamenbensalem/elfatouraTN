using System;
using System.Collections.Generic;

namespace GestCom.Application.Features.Achats.DemandesPrix.DTOs;

/// <summary>
/// DTO complet pour une demande de prix
/// </summary>
public class DemandePrixDto
{
    public string NumeroDemande { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public DateTime DateDemande { get; set; }
    public DateTime? DateValidite { get; set; }
    
    // Fournisseur
    public string CodeFournisseur { get; set; } = string.Empty;
    public string? NomFournisseur { get; set; }
    public string? AdresseFournisseur { get; set; }
    
    // Statut
    public string? Statut { get; set; }
    public string? Observations { get; set; }
    
    // Conversion
    public string? NumeroCommande { get; set; }
    public bool EstConvertie => !string.IsNullOrEmpty(NumeroCommande);
    
    // Lignes
    public List<LigneDemandePrixDto> Lignes { get; set; } = new();
    
    // Audit
    public DateTime DateCreation { get; set; }
    public DateTime? DateModification { get; set; }
}

/// <summary>
/// DTO pour une ligne de demande de prix
/// </summary>
public class LigneDemandePrixDto
{
    public int NumeroLigne { get; set; }
    public string CodeProduit { get; set; } = string.Empty;
    public string? DesignationProduit { get; set; }
    public decimal Quantite { get; set; }
    public decimal? PrixPropose { get; set; }
    public string? Observations { get; set; }
}

/// <summary>
/// DTO simplifié pour les listes
/// </summary>
public class DemandePrixListDto
{
    public string NumeroDemande { get; set; } = string.Empty;
    public DateTime DateDemande { get; set; }
    public DateTime? DateValidite { get; set; }
    public string CodeFournisseur { get; set; } = string.Empty;
    public string? NomFournisseur { get; set; }
    public string? Statut { get; set; }
    public bool EstConvertie { get; set; }
    public int NombreLignes { get; set; }
}

/// <summary>
/// DTO pour la création d'une demande de prix
/// </summary>
public class CreateDemandePrixDto
{
    public DateTime DateDemande { get; set; } = DateTime.Now;
    public DateTime? DateValidite { get; set; }
    public string CodeFournisseur { get; set; } = string.Empty;
    public string? Observations { get; set; }
    
    public List<CreateLigneDemandePrixDto> Lignes { get; set; } = new();
}

/// <summary>
/// DTO pour la création d'une ligne
/// </summary>
public class CreateLigneDemandePrixDto
{
    public string CodeProduit { get; set; } = string.Empty;
    public decimal Quantite { get; set; }
    public string? Observations { get; set; }
}
