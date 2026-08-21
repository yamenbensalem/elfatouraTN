using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_T4C_GestCom.Data.Models;

[Table("fournisseur")]
public class Fournisseur : ITenantOwned
{
    [Key]
    [Column("code_fournisseur")]
    [MaxLength(20)]
    public string CodeFournisseur { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    [Column("nom_fournisseur")]
    [Display(Name = "Nom")]
    public string NomFournisseur { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("matriculefiscale_fournisseur")]
    [Display(Name = "Matricule Fiscale")]
    public string? MatriculeFiscale { get; set; }

    [MaxLength(300)]
    [Column("adresse_fournisseur")]
    [Display(Name = "Adresse")]
    public string? Adresse { get; set; }

    [MaxLength(20)]
    [Column("codepostal_fournisseur")]
    [Display(Name = "Code Postal")]
    public string? CodePostal { get; set; }

    [MaxLength(100)]
    [Column("ville_fournisseur")]
    [Display(Name = "Ville")]
    public string? Ville { get; set; }

    [MaxLength(100)]
    [Column("pays_fournisseur")]
    [Display(Name = "Pays")]
    public string? Pays { get; set; }

    [MaxLength(30)]
    [Column("tel_fournisseur")]
    [Display(Name = "Téléphone")]
    public string? Tel { get; set; }

    [MaxLength(30)]
    [Column("telmobile_fournisseur")]
    [Display(Name = "Mobile")]
    public string? TelMobile { get; set; }

    [MaxLength(30)]
    [Column("fax_fournisseur")]
    [Display(Name = "Fax")]
    public string? Fax { get; set; }

    [MaxLength(100)]
    [Column("email_fournisseur")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [MaxLength(200)]
    [Column("rib_fournisseur")]
    [Display(Name = "RIB")]
    public string? Rib { get; set; }

    [MaxLength(10)]
    [Column("etat_fournisseur")]
    [Display(Name = "État")]
    public string EtatFournisseur { get; set; } = "Actif";

    [MaxLength(500)]
    [Column("note_fournisseur")]
    [Display(Name = "Note")]
    public string? Note { get; set; }

    [Column("code_devise")]
    [Display(Name = "Devise")]
    public int CodeDevise { get; set; } = 1;

    [Column("company_id_fournisseur")]
    public int? CompanyId { get; set; }

    // Navigation
    [ForeignKey(nameof(CodeDevise))]
    public Devise? Devise { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public Company? Company { get; set; }

    public ICollection<CommandeAchat> CommandesAchat { get; set; } = [];
    public ICollection<BonReception> BonsReception { get; set; } = [];
    public ICollection<FactureFournisseur> FacturesFournisseur { get; set; } = [];
}
