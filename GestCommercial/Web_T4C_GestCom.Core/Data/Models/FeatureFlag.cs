using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_T4C_GestCom.Data.Models;

/// <summary>
/// Activation/désactivation d'une fonctionnalité par entreprise.
/// Permet de contrôler l'accès à un module indépendamment des permissions RBAC.
/// </summary>
[Table("feature_flag")]
public class FeatureFlag
{
    [Key]
    [Column("id_feature_flag")]
    public int Id { get; set; }

    [Column("company_id_flag")]
    public int CompanyId { get; set; }

    /// <summary>Nom du module (ex. "factures", "devis", "rapports-avances").</summary>
    [Required, MaxLength(100)]
    [Column("feature_name_flag")]
    public string Feature { get; set; } = string.Empty;

    /// <summary>true = fonctionnalité activée pour cette entreprise.</summary>
    [Column("is_enabled_flag")]
    public bool IsEnabled { get; set; } = true;

    // Navigation
    [ForeignKey(nameof(CompanyId))]
    public Company? Company { get; set; }
}
