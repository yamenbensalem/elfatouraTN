using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_GestCom.Data.Models;

/// <summary>
/// Platform-level webhook record — SuperAdmin-managed storage only (no delivery mechanism wired
/// to it by design). CompanyId is an optional tag, not tenant ownership: SuperAdmin sees and
/// manages every row regardless of company, so this entity is intentionally NOT ITenantOwned.
/// </summary>
[Table("webhook")]
public class Webhook
{
    [Key]
    [Column("id_webhook")]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    [Column("name_webhook")]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    [Column("url_webhook")]
    public string Url { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("event_webhook")]
    public string EventType { get; set; } = string.Empty;

    /// <summary>Optional company this webhook is associated with; null = platform-wide.</summary>
    [Column("company_id_webhook")]
    public int? CompanyId { get; set; }

    [Column("active_webhook")]
    public bool Active { get; set; } = true;

    [Column("date_creation_webhook")]
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CompanyId))]
    public Company? Company { get; set; }
}
