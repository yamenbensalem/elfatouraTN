using System.ComponentModel.DataAnnotations.Schema;

namespace Web_GestCom.Data.Models;

/// <summary>Table de jointure AppRole ↔ Permission (relation N:N).</summary>
[Table("role_permission")]
public class RolePermission
{
    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("permission_id")]
    public int PermissionId { get; set; }

    // Navigation
    [ForeignKey(nameof(RoleId))]
    public AppRole? Role { get; set; }

    [ForeignKey(nameof(PermissionId))]
    public Permission? Permission { get; set; }
}
