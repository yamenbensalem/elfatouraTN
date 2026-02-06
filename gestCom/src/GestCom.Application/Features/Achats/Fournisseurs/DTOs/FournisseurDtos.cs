using System;

namespace GestCom.Application.Features.Achats.Fournisseurs.DTOs;

/// <summary>
/// DTO complet pour un fournisseur
/// </summary>
public class FournisseurDto
{
    public string CodeFournisseur { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public string? RaisonSociale { get; set; }
    public string? MatriculeFiscale { get; set; }
    public string? Adresse { get; set; }
    public string? CodePostal { get; set; }
    public string? Ville { get; set; }
    public string? Pays { get; set; }
    public string? Telephone { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? SiteWeb { get; set; }
    public string? Contact { get; set; }
    public string? Observations { get; set; }
    
    // Conditions commerciales
    public decimal DelaiPaiement { get; set; }
    public decimal TauxRemise { get; set; }
    
    // Statistiques
    public decimal TotalAchats { get; set; }
    public decimal TotalDettes { get; set; }
    public int NombreCommandes { get; set; }
    public int NombreFactures { get; set; }
    
    // Audit
    public DateTime DateCreation { get; set; }
    public DateTime? DateModification { get; set; }
}

/// <summary>
/// DTO simplifié pour les listes de fournisseurs
/// </summary>
public class FournisseurListDto
{
    public string CodeFournisseur { get; set; } = string.Empty;
    public string? RaisonSociale { get; set; }
    public string? MatriculeFiscale { get; set; }
    public string? Telephone { get; set; }
    public string? Email { get; set; }
    public string? Ville { get; set; }
    public decimal TotalDettes { get; set; }
}

/// <summary>
/// DTO pour la création d'un fournisseur
/// </summary>
public class CreateFournisseurDto
{
    public string CodeFournisseur { get; set; } = string.Empty;
    public string? RaisonSociale { get; set; }
    public string? MatriculeFiscale { get; set; }
    public string? Adresse { get; set; }
    public string? CodePostal { get; set; }
    public string? Ville { get; set; }
    public string? Pays { get; set; }
    public string? Telephone { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? SiteWeb { get; set; }
    public string? Contact { get; set; }
    public string? Observations { get; set; }
    public decimal DelaiPaiement { get; set; }
    public decimal TauxRemise { get; set; }
}

/// <summary>
/// DTO pour la mise à jour d'un fournisseur
/// </summary>
public class UpdateFournisseurDto
{
    public string CodeFournisseur { get; set; } = string.Empty;
    public string? RaisonSociale { get; set; }
    public string? MatriculeFiscale { get; set; }
    public string? Adresse { get; set; }
    public string? CodePostal { get; set; }
    public string? Ville { get; set; }
    public string? Pays { get; set; }
    public string? Telephone { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? SiteWeb { get; set; }
    public string? Contact { get; set; }
    public string? Observations { get; set; }
    public decimal DelaiPaiement { get; set; }
    public decimal TauxRemise { get; set; }
}
