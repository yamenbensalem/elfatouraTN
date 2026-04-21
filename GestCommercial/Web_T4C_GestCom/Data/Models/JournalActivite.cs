using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_T4C_GestCom.Data.Models;

[Table("journalactivite")]
public class JournalActivite
{
    [Key]
    [Column("id_journal")]
    public int Id { get; set; }

    [Required, MaxLength(50)]
    [Column("login_journal")]
    public string Login { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    [Column("action_journal")]
    public string Action { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    [Column("entite_journal")]
    public string Entite { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column("codeentite_journal")]
    public string? CodeEntite { get; set; }

    [Column("dateheure_journal")]
    public DateTime DateHeure { get; set; } = DateTime.Now;

    [MaxLength(255)]
    [Column("detail_journal")]
    public string? Detail { get; set; }

    [Column("company_id_journal")]
    public int? CompanyId { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public Company? Company { get; set; }
}
