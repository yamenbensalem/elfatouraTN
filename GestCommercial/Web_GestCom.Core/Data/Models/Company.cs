using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_GestCom.Data.Models;

/// <summary>Entité tenant : représente une entreprise/organisation dans le système multi-tenant.</summary>
[Table("company")]
public class Company
{
    [Key]
    [Column("id_company")]
    public int Id { get; set; }

    /// <summary>Nom affiché de l'entreprise.</summary>
    [Required, MaxLength(200)]
    [Column("name_company")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Identifiant URL unique (ex. "societe-abc").</summary>
    [MaxLength(100)]
    [Column("slug_company")]
    public string? Slug { get; set; }

    /// <summary>Plan tarifaire : Standard, Pro, Enterprise.</summary>
    [MaxLength(50)]
    [Column("plan_company")]
    public string Plan { get; set; } = "Standard";

    /// <summary>Paramètres JSON libres (thème, devise par défaut, etc.).</summary>
    [Column("settings_company")]
    public string? SettingsJson { get; set; }

    /// <summary>
    /// Quotas de comptes que l'Admin de cette entreprise peut créer/promouvoir lui-même en
    /// libre-service, par rôle. Null = illimité. Au-delà du quota, seul le SuperAdmin peut créer
    /// un nouveau compte de ce rôle pour cette entreprise (voir UtilisateurService). Seedés selon
    /// le Plan à la création (CompanyService.AddAsync), puis librement modifiables au cas par cas.
    /// </summary>
    [Column("max_admins_company")]
    public int? MaxAdmins { get; set; }

    [Column("max_managers_company")]
    public int? MaxManagers { get; set; }

    [Column("max_employes_company")]
    public int? MaxEmployes { get; set; }

    // Navigation
    public ICollection<Utilisateur> Utilisateurs { get; set; } = [];
    public ICollection<AppRole>     Roles         { get; set; } = [];
    public ICollection<FeatureFlag> FeatureFlags  { get; set; } = [];
    public ICollection<ApiKey>      ApiKeys       { get; set; } = [];
    public ICollection<Webhook>     Webhooks      { get; set; } = [];
}
