using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_GestCom.Data.Models;

/// <summary>
/// Permission atomique. Le code complet est "Feature.Action" (ex. "factures.view").
/// Feature = module (factures, devis, …), Action = opération (view, create, update, delete).
/// </summary>
[Table("permission")]
public class Permission
{
    [Key]
    [Column("id_permission")]
    public int Id { get; set; }

    /// <summary>Module concerné (ex. "factures", "devis", "commandes-vente").</summary>
    [Required, MaxLength(100)]
    [Column("feature_permission")]
    public string Feature { get; set; } = string.Empty;

    /// <summary>Opération (view | create | update | delete).</summary>
    [Required, MaxLength(50)]
    [Column("action_permission")]
    public string Action { get; set; } = string.Empty;

    /// <summary>Code complet calculé : "Feature.Action" (ex. "factures.view").</summary>
    [NotMapped]
    public string Code => $"{Feature}.{Action}";

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
