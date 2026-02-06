using System;
using System.Collections.Generic;

namespace GestCom.Application.Features.Ventes.Factures.DTOs;

/// <summary>
/// DTO complet pour une facture client
/// </summary>
public class FactureClientDto
{
    public string NumeroFacture { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public DateTime DateFacture { get; set; }
    public DateTime? DateEcheance { get; set; }
    
    // Client
    public string CodeClient { get; set; } = string.Empty;
    public string? NomClient { get; set; }
    public string? AdresseClient { get; set; }
    public string? MatriculeFiscalClient { get; set; }
    
    // Montants
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }
    public decimal MontantFODEC { get; set; }
    public decimal Timbre { get; set; }
    public decimal Remise { get; set; }
    public decimal TauxRemise { get; set; }
    
    // Retenue à la source (RAS)
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
    public string? NumeroBonLivraison { get; set; }
    public string? NumeroCommande { get; set; }
    
    // Devise
    public string? CodeDevise { get; set; }
    public string? SymboleDevise { get; set; }
    public decimal TauxChange { get; set; }
    
    // Mode de paiement
    public string? CodeModePaiement { get; set; }
    public string? LibelleModePaiement { get; set; }
    
    // Lignes
    public List<LigneFactureClientDto> Lignes { get; set; } = new();
    
    // Audit
    public DateTime DateCreation { get; set; }
    public DateTime? DateModification { get; set; }
}

/// <summary>
/// DTO pour une ligne de facture
/// </summary>
public class LigneFactureClientDto
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
    public decimal TauxFODEC { get; set; }
    public decimal MontantFODEC { get; set; }
}

/// <summary>
/// DTO simplifié pour les listes de factures
/// </summary>
public class FactureClientListDto
{
    public string NumeroFacture { get; set; } = string.Empty;
    public DateTime DateFacture { get; set; }
    public string CodeClient { get; set; } = string.Empty;
    public string? NomClient { get; set; }
    public decimal MontantTTC { get; set; }
    public decimal NetAPayer { get; set; }
    public decimal MontantRegle { get; set; }
    public decimal Reste => NetAPayer - MontantRegle;
    public bool EstPayee => Reste <= 0;
    public string? Statut { get; set; }
    public int NombreLignes { get; set; }
}

/// <summary>
/// DTO pour la création d'une facture
/// </summary>
public class CreateFactureClientDto
{
    public DateTime DateFacture { get; set; } = DateTime.Now;
    public DateTime? DateEcheance { get; set; }
    public string CodeClient { get; set; } = string.Empty;
    
    public decimal TauxRemise { get; set; }
    public decimal Timbre { get; set; }
    public decimal TauxRAS { get; set; }
    
    public string? CodeDevise { get; set; }
    public decimal TauxChange { get; set; } = 1;
    public string? CodeModePaiement { get; set; }
    
    public string? NumeroBonLivraison { get; set; }
    public string? NumeroCommande { get; set; }
    public string? Observations { get; set; }
    
    public List<CreateLigneFactureClientDto> Lignes { get; set; } = new();
}

/// <summary>
/// DTO pour la création d'une ligne de facture
/// </summary>
public class CreateLigneFactureClientDto
{
    public string CodeProduit { get; set; } = string.Empty;
    public decimal Quantite { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal TauxTVA { get; set; }
    public decimal TauxRemise { get; set; }
    public decimal TauxFODEC { get; set; }
}

/// <summary>
/// DTO pour la mise à jour d'une facture
/// </summary>
public class UpdateFactureClientDto
{
    public string NumeroFacture { get; set; } = string.Empty;
    public DateTime DateFacture { get; set; }
    public DateTime? DateEcheance { get; set; }
    
    public decimal TauxRemise { get; set; }
    public decimal Timbre { get; set; }
    public decimal TauxRAS { get; set; }
    
    public string? CodeDevise { get; set; }
    public decimal TauxChange { get; set; } = 1;
    public string? CodeModePaiement { get; set; }
    
    public string? Observations { get; set; }
    
    public List<UpdateLigneFactureClientDto> Lignes { get; set; } = new();
}

/// <summary>
/// DTO pour la mise à jour d'une ligne de facture
/// </summary>
public class UpdateLigneFactureClientDto
{
    public int? NumeroLigne { get; set; } // null = nouvelle ligne
    public string CodeProduit { get; set; } = string.Empty;
    public decimal Quantite { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal TauxTVA { get; set; }
    public decimal TauxRemise { get; set; }
    public decimal TauxFODEC { get; set; }
}

/// <summary>
/// DTO pour le rapport de chiffre d'affaires
/// </summary>
public class ChiffreAffairesDto
{
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public decimal TotalHT { get; set; }
    public decimal TotalTVA { get; set; }
    public decimal TotalTTC { get; set; }
    public decimal TotalRegle { get; set; }
    public decimal TotalImpaye { get; set; }
    public int NombreFactures { get; set; }
    public List<ChiffreAffairesParMoisDto> ParMois { get; set; } = new();
}

/// <summary>
/// DTO pour le CA par mois
/// </summary>
public class ChiffreAffairesParMoisDto
{
    public int Annee { get; set; }
    public int Mois { get; set; }
    public decimal MontantHT { get; set; }
    public decimal MontantTTC { get; set; }
    public int NombreFactures { get; set; }
}
