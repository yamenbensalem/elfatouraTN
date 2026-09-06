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

    // Navigation
    public ICollection<Utilisateur> Utilisateurs { get; set; } = [];
    public ICollection<AppRole>     Roles         { get; set; } = [];
    public ICollection<FeatureFlag> FeatureFlags  { get; set; } = [];
}
