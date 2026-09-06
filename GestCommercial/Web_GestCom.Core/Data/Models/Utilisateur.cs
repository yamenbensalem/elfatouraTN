using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_GestCom.Data.Models;

[Table("utilisateurs")]
public class Utilisateur : ITenantOwned
{
    [Key]
    [Column("id_utilisateur")]
    public int Id { get; set; }

    [Required, MaxLength(50)]
    [Column("login_utilisateur")]
    public string Login { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("passwordhash_utilisateur")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    [Column("prenom_utilisateur")]
    public string Prenom { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    [Column("nom_utilisateur")]
    public string Nom { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("email_utilisateur")]
    public string? Email { get; set; }

    [Obsolete("Use UserRoles/AppRole for role resolution. This column will be removed in a future migration.")]
    [Required, MaxLength(20)]
    [Column("role_utilisateur")]
    public string Role { get; set; } = "Employé";

    [Column("actif_utilisateur")]
    public bool Actif { get; set; } = true;

    [Column("datecreation_utilisateur")]
    public DateTime DateCreation { get; set; } = DateTime.Now;

    /// <summary>Tenant auquel appartient cet utilisateur. Null = utilisateur système global.</summary>
    [Column("company_id_utilisateur")]
    public int? CompanyId { get; set; }

    [Column("is_superadmin_utilisateur")]
    public bool IsSuperAdmin { get; set; } = false;

    [Required, MaxLength(64)]
    [Column("securitystamp_utilisateur")]
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");

    [Column("permissionsversion_utilisateur")]
    public int PermissionsVersion { get; set; } = 1;

    [NotMapped]
    public string NomComplet => $"{Prenom} {Nom}";

    // Navigation RBAC
    [ForeignKey(nameof(CompanyId))]
    public Company? Company { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
}
