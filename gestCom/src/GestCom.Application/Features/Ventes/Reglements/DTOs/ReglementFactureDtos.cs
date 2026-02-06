using System;

namespace GestCom.Application.Features.Ventes.Reglements.DTOs;

/// <summary>
/// DTO pour un règlement de facture client
/// </summary>
public class ReglementFactureDto
{
    public string NumeroReglement { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public DateTime DateReglement { get; set; }
    
    // Facture
    public string NumeroFacture { get; set; } = string.Empty;
    public decimal MontantFacture { get; set; }
    public decimal ResteARegler { get; set; }
    
    // Client
    public string CodeClient { get; set; } = string.Empty;
    public string? NomClient { get; set; }
    
    // Montant
    public decimal Montant { get; set; }
    
    // Mode de paiement
    public string? CodeModePaiement { get; set; }
    public string? LibelleModePaiement { get; set; }
    
    // Référence paiement (numéro chèque, virement, etc.)
    public string? Reference { get; set; }
    public string? Banque { get; set; }
    public DateTime? DateEcheance { get; set; }
    
    // Observations
    public string? Observations { get; set; }
    
    // Audit
    public DateTime DateCreation { get; set; }
    public DateTime? DateModification { get; set; }
}

/// <summary>
/// DTO simplifié pour les listes
/// </summary>
public class ReglementFactureListDto
{
    public string NumeroReglement { get; set; } = string.Empty;
    public DateTime DateReglement { get; set; }
    public string NumeroFacture { get; set; } = string.Empty;
    public string CodeClient { get; set; } = string.Empty;
    public string? NomClient { get; set; }
    public decimal Montant { get; set; }
    public string? LibelleModePaiement { get; set; }
    public string? Reference { get; set; }
}

/// <summary>
/// DTO pour la création d'un règlement
/// </summary>
public class CreateReglementFactureDto
{
    public DateTime DateReglement { get; set; } = DateTime.Now;
    public string NumeroFacture { get; set; } = string.Empty;
    public decimal Montant { get; set; }
    public string? CodeModePaiement { get; set; }
    public string? Reference { get; set; }
    public string? Banque { get; set; }
    public DateTime? DateEcheance { get; set; }
    public string? Observations { get; set; }
}

/// <summary>
/// DTO pour le résumé des règlements d'un client
/// </summary>
public class ResumeReglementsClientDto
{
    public string CodeClient { get; set; } = string.Empty;
    public string? NomClient { get; set; }
    public decimal TotalFactures { get; set; }
    public decimal TotalReglements { get; set; }
    public decimal SoldeCreances { get; set; }
    public int NombreFacturesImpayees { get; set; }
    public List<ReglementFactureListDto> DerniersReglements { get; set; } = new();
}
