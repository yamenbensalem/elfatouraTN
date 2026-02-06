using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité Entreprise - Configuration de la société
/// </summary>
public class Entreprise : BaseEntity
{
    public string CodeEntreprise { get; set; } = string.Empty;
    public string MatriculeFiscale { get; set; } = string.Empty;
    public string RaisonSociale { get; set; } = string.Empty;
    public string? NomCommercial { get; set; }
    public string TypePersonne { get; set; } = string.Empty; // Personne Physique / Personne Morale
    public string? CompteBancaire { get; set; }
    public string? RegistreCommerce { get; set; }
    public decimal CapitalSocial { get; set; }
    public string? Description { get; set; }
    
    // Adresse
    public string Adresse { get; set; } = string.Empty;
    public string? CodePostal { get; set; }
    public string Ville { get; set; } = string.Empty;
    public string Pays { get; set; } = "Tunisie";
    
    // Contact
    public string? TelFixe1 { get; set; }
    public string? TelFixe2 { get; set; }
    public string? TelMobile { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? SiteWeb { get; set; }
    
    // Identifiants fiscaux
    public string? Matricule { get; set; }
    public string? CodeTVA { get; set; }
    public string? CodeCategorie { get; set; }
    public string? NumeroEtablissement { get; set; }
    public bool AssujittieTVA { get; set; }
    public bool AssujittieFodec { get; set; }
    public bool Exonore { get; set; }
    public string? CodeDouane { get; set; }
    
    // Configuration
    public int CodeDevise { get; set; }
    public string? Logo { get; set; }
    
    // Banque
    public string? RIB { get; set; }
    public string? NomBanque { get; set; }

    // Alias for Application layer compatibility
    public string MatriculeFiscal { get => MatriculeFiscale; set => MatriculeFiscale = value; }
    public string? Telephone { get => TelFixe1; set => TelFixe1 = value; }

    // Navigation properties
    public Devise? Devise { get; set; }
}
