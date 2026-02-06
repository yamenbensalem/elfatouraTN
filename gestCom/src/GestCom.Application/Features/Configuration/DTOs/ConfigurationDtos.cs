namespace GestCom.Application.Features.Configuration.DTOs;

/// <summary>
/// DTO pour les catégories de produits
/// </summary>
public class CategorieProduitDto
{
    public string CodeCategorie { get; set; } = string.Empty;
    public string LibelleCategorie { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int NombreProduits { get; set; }
    public bool EstActif { get; set; } = true;
}

/// <summary>
/// DTO pour les taux de TVA
/// </summary>
public class TvaProduitDto
{
    public string CodeTva { get; set; } = string.Empty;
    public string LibelleTva { get; set; } = string.Empty;
    public decimal TauxTva { get; set; }
    public bool EstDefaut { get; set; }
}

/// <summary>
/// DTO pour les devises
/// </summary>
public class DeviseDto
{
    public string CodeDevise { get; set; } = string.Empty;
    public string LibelleDevise { get; set; } = string.Empty;
    public string? Symbole { get; set; }
    public decimal TauxChange { get; set; } = 1.0m;
}

/// <summary>
/// DTO pour les unités de mesure
/// </summary>
public class UniteProduitDto
{
    public string CodeUnite { get; set; } = string.Empty;
    public string LibelleUnite { get; set; } = string.Empty;
}

/// <summary>
/// DTO pour les magasins
/// </summary>
public class MagasinProduitDto
{
    public string CodeMagasin { get; set; } = string.Empty;
    public string LibelleMagasin { get; set; } = string.Empty;
    public string? Adresse { get; set; }
    public string? Responsable { get; set; }
    public int NombreProduits { get; set; }
    public decimal ValeurStock { get; set; }
    public bool EstDefaut { get; set; }
    public bool EstActif { get; set; } = true;
}

/// <summary>
/// DTO pour les modes de paiement
/// </summary>
public class ModePayementDto
{
    public string CodeModePaiement { get; set; } = string.Empty;
    public string LibelleModePaiement { get; set; } = string.Empty;
    public bool NecessiteReference { get; set; }
}

/// <summary>
/// Alias pour compatibilité (ModePayement vs ModePaiement)
/// </summary>
public class ModePaiementDto : ModePayementDto { }

/// <summary>
/// DTO pour l'entreprise
/// </summary>
public class EntrepriseDto
{
    public string CodeEntreprise { get; set; } = string.Empty;
    public string RaisonSociale { get; set; } = string.Empty;
    public string? Adresse { get; set; }
    public string? CodePostal { get; set; }
    public string? Ville { get; set; }
    public string? Pays { get; set; }
    public string? Telephone { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? SiteWeb { get; set; }
    public string? MatriculeFiscale { get; set; }
    public string? RegistreCommerce { get; set; }
    public string? CodeTVA { get; set; }
    public string? Logo { get; set; }
    public string? DeviseDefaut { get; set; }
}

/// <summary>
/// DTO pour la mise à jour de catégorie
/// </summary>
public class UpdateCategorieProduitDto
{
    public string LibelleCategorie { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool EstActif { get; set; } = true;
}

/// <summary>
/// DTO pour la mise à jour de TVA
/// </summary>
public class UpdateTvaProduitDto
{
    public string LibelleTva { get; set; } = string.Empty;
    public decimal TauxTva { get; set; }
    public bool EstDefaut { get; set; }
}
