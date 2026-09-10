using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_GestCom.Data.Models;

/// <summary>
/// Platform-level API key record — SuperAdmin-managed storage only (no real API/auth wired
/// to it by design). CompanyId is an optional tag, not tenant ownership: SuperAdmin sees and
/// manages every row regardless of company, so this entity is intentionally NOT ITenantOwned.
/// </summary>
[Table("api_key")]
public class ApiKey
{
    [Key]
    [Column("id_api_key")]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    [Column("name_api_key")]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    [Column("value_api_key")]
    public string Value { get; set; } = string.Empty;

    /// <summary>Optional company this key is associated with; null = platform-wide.</summary>
    [Column("company_id_api_key")]
    public int? CompanyId { get; set; }

    [Column("active_api_key")]
    public bool Active { get; set; } = true;

    [Column("date_creation_api_key")]
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CompanyId))]
    public Company? Company { get; set; }
}
