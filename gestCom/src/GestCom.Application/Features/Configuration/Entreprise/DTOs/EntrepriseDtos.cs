using System;

namespace GestCom.Application.Features.Configuration.Entreprise.DTOs;

/// <summary>
/// DTO complet pour l'entreprise
/// </summary>
public class EntrepriseDto
{
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
    
    // Configuration fiscale
    public decimal TauxTVA { get; set; }
    public decimal TauxTimbre { get; set; }
    public decimal Timbre { get; set; }
    public bool AssujettiFodec { get; set; }
    
    // Devise par défaut
    public string? CodeDevise { get; set; }
    public string? SymboleDevise { get; set; }
    
    // Registre commerce
    public string? RegistreCommerce { get; set; }
    
    // Logo
    public string? LogoPath { get; set; }
    
    // Audit
    public DateTime DateCreation { get; set; }
    public DateTime? DateModification { get; set; }
}

/// <summary>
/// DTO pour la mise à jour de l'entreprise
/// </summary>
public class UpdateEntrepriseDto
{
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
    
    public decimal TauxTVA { get; set; }
    public decimal TauxTimbre { get; set; }
    public decimal Timbre { get; set; }
    public bool AssujettiFodec { get; set; }
    
    public string? CodeDevise { get; set; }
    public string? RegistreCommerce { get; set; }
}
