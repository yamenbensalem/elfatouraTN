using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_GestCom.Data.Models;

[Table("entreprise")]
public class Entreprise
{
    [Key]
    [Column("code_entreprise")]
    public string CodeEntreprise { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    [Column("nom_entreprise")]
    public string NomEntreprise { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("matriculefiscale_entreprise")]
    public string? MatriculeFiscale { get; set; }

    [MaxLength(300)]
    [Column("adresse_entreprise")]
    public string? Adresse { get; set; }

    [MaxLength(20)]
    [Column("codepostal_entreprise")]
    public string? CodePostal { get; set; }

    [MaxLength(100)]
    [Column("ville_entreprise")]
    public string? Ville { get; set; }

    [MaxLength(100)]
    [Column("pays_entreprise")]
    public string? Pays { get; set; }

    [MaxLength(30)]
    [Column("tel_entreprise")]
    public string? Tel { get; set; }

    [MaxLength(30)]
    [Column("fax_entreprise")]
    public string? Fax { get; set; }

    [MaxLength(100)]
    [Column("email_entreprise")]
    public string? Email { get; set; }

    [MaxLength(200)]
    [Column("site_entreprise")]
    public string? Site { get; set; }

    [MaxLength(5)]
    [Column("logo_entreprise")]
    public string? Logo { get; set; }

    [MaxLength(300)]
    [Column("pathlogo_entreprise")]
    public string? PathLogo { get; set; }

    [MaxLength(200)]
    [Column("rib_entreprise")]
    public string? Rib { get; set; }

    [MaxLength(200)]
    [Column("note_entreprise")]
    public string? Note { get; set; }
}
