using System.ComponentModel.DataAnnotations.Schema;

namespace Web_T4C_GestCom.Data.Models;

/// <summary>Table de jointure Utilisateur ↔ AppRole (relation N:N).</summary>
[Table("user_role")]
public class UserRole
{
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("role_id")]
    public int RoleId { get; set; }

    // Navigation
    [ForeignKey(nameof(UserId))]
    public Utilisateur? Utilisateur { get; set; }

    [ForeignKey(nameof(RoleId))]
    public AppRole? Role { get; set; }
}
