using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_GestCom.Data.Models;

/// <summary>
/// Rôle RBAC. CompanyId null = rôle global (Admin, Manager, Employé, SuperAdmin prédéfinis).
/// Chaque entreprise peut créer ses propres rôles (CompanyId non null).
/// </summary>
[Table("app_role")]
public class AppRole
{
    [Key]
    [Column("id_role")]
    public int Id { get; set; }

    /// <summary>Nom du rôle (ex. "Admin", "Manager", "Employé", "Commercial").</summary>
    [Required, MaxLength(100)]
    [Column("name_role")]
    public string Name { get; set; } = string.Empty;

    /// <summary>null = rôle système global ; valeur = rôle spécifique à une entreprise.</summary>
    [Column("company_id_role")]
    public int? CompanyId { get; set; }

    // Navigation
    [ForeignKey(nameof(CompanyId))]
    public Company? Company { get; set; }

    public ICollection<UserRole>        UserRoles       { get; set; } = [];
    public ICollection<RolePermission>  RolePermissions { get; set; } = [];
}
