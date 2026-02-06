using System;
using System.Collections.Generic;

namespace GestCom.Application.Features.Ventes.Devis.DTOs;

/// <summary>
/// DTO complet pour un devis client
/// </summary>
public class DevisClientDto
{
    public string NumeroDevis { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public DateTime DateDevis { get; set; }
    public DateTime? DateValidite { get; set; }
    
    // Client
    public string CodeClient { get; set; } = string.Empty;
    public string? NomClient { get; set; }
    public string? AdresseClient { get; set; }
    
    // Montants
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }
    public decimal MontantFODEC { get; set; }
    public decimal Timbre { get; set; }
    public decimal Remise { get; set; }
    public decimal TauxRemise { get; set; }
    
    // Statut
    public string? Statut { get; set; }
    public bool EstExpire => DateValidite.HasValue && DateValidite.Value < DateTime.Now;
    public string? Observations { get; set; }
    
    // Conversion
    public string? NumeroCommande { get; set; }
    public bool EstConverti => !string.IsNullOrEmpty(NumeroCommande);
    
    // Devise
    public string? CodeDevise { get; set; }
    public decimal TauxChange { get; set; }
    
    // Lignes
    public List<LigneDevisClientDto> Lignes { get; set; } = new();
    
    // Audit
    public DateTime DateCreation { get; set; }
    public DateTime? DateModification { get; set; }
}

/// <summary>
/// DTO pour une ligne de devis
/// </summary>
public class LigneDevisClientDto
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
/// DTO simplifié pour les listes de devis
/// </summary>
public class DevisClientListDto
{
    public string NumeroDevis { get; set; } = string.Empty;
    public DateTime DateDevis { get; set; }
    public DateTime? DateValidite { get; set; }
    public string CodeClient { get; set; } = string.Empty;
    public string? NomClient { get; set; }
    public decimal MontantTTC { get; set; }
    public string? Statut { get; set; }
    public bool EstExpire => DateValidite.HasValue && DateValidite.Value < DateTime.Now;
    public bool EstConverti { get; set; }
    public int NombreLignes { get; set; }
}

/// <summary>
/// DTO pour la création d'un devis
/// </summary>
public class CreateDevisClientDto
{
    public DateTime DateDevis { get; set; } = DateTime.Now;
    public DateTime? DateValidite { get; set; }
    public string CodeClient { get; set; } = string.Empty;
    
    public decimal TauxRemise { get; set; }
    public decimal Timbre { get; set; }
    
    public string? CodeDevise { get; set; }
    public decimal TauxChange { get; set; } = 1;
    public string? Observations { get; set; }
    
    public List<CreateLigneDevisClientDto> Lignes { get; set; } = new();
}

/// <summary>
/// DTO pour la création d'une ligne de devis
/// </summary>
public class CreateLigneDevisClientDto
{
    public string CodeProduit { get; set; } = string.Empty;
    public decimal Quantite { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal TauxTVA { get; set; }
    public decimal TauxRemise { get; set; }
}

/// <summary>
/// DTO pour la conversion d'un devis en commande
/// </summary>
public class ConvertDevisToCommandeDto
{
    public string NumeroDevis { get; set; } = string.Empty;
    public DateTime? DateLivraisonPrevue { get; set; }
    public string? Observations { get; set; }
}
