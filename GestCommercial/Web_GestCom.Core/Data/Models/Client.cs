using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_GestCom.Data.Models;

[Table("client")]
public class Client : ITenantOwned
{
    [Key]
    [Column("code_client")]
    [MaxLength(20)]
    public string CodeClient { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    [Column("nom_client")]
    [Display(Name = "Nom")]
    public string NomClient { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("matriculefiscale_client")]
    [Display(Name = "Matricule Fiscale")]
    public string? MatriculeFiscale { get; set; }

    [MaxLength(20)]
    [Column("typepersonne_client")]
    [Display(Name = "Type Personne")]
    public string TypePersonne { get; set; } = "Physique";  // "Physique" or "Morale"

    [MaxLength(50)]
    [Column("typeentreprise_client")]
    [Display(Name = "Type Entreprise")]
    public string? TypeEntreprise { get; set; }

    [MaxLength(200)]
    [Column("rib_client")]
    [Display(Name = "RIB")]
    public string? Rib { get; set; }

    [MaxLength(300)]
    [Column("adresse_client")]
    [Display(Name = "Adresse")]
    public string? Adresse { get; set; }

    [MaxLength(20)]
    [Column("codepostal_client")]
    [Display(Name = "Code Postal")]
    public string? CodePostal { get; set; }

    [MaxLength(100)]
    [Column("ville_client")]
    [Display(Name = "Ville")]
    public string? Ville { get; set; }

    [MaxLength(100)]
    [Column("pays_client")]
    [Display(Name = "Pays")]
    public string? Pays { get; set; }

    [MaxLength(30)]
    [Column("tel_client")]
    [Display(Name = "Téléphone")]
    public string? Tel { get; set; }

    [MaxLength(30)]
    [Column("telmobile_client")]
    [Display(Name = "Mobile")]
    public string? TelMobile { get; set; }

    [MaxLength(30)]
    [Column("fax_client")]
    [Display(Name = "Fax")]
    public string? Fax { get; set; }

    [MaxLength(100)]
    [Column("email_client")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [MaxLength(200)]
    [Column("site_client")]
    [Display(Name = "Site Web")]
    public string? Site { get; set; }

    [MaxLength(10)]
    [Column("etat_client")]
    [Display(Name = "État")]
    public string EtatClient { get; set; } = "Actif";

    [Column("nbtransactions_client")]
    public int NbTransactions { get; set; } = 0;

    [MaxLength(500)]
    [Column("note_client")]
    [Display(Name = "Note")]
    public string? Note { get; set; }

    [MaxLength(5)]
    [Column("etranger_client")]
    [Display(Name = "Étranger")]
    public string Etranger { get; set; } = "NON";   // "OUI" or "NON"

    [MaxLength(5)]
    [Column("exonore_client")]
    [Display(Name = "Exonéré TVA")]
    public string Exonore { get; set; } = "NON";    // "OUI" or "NON" — VAT exempt

    [Column("maxcredit_client")]
    [Display(Name = "Crédit Maximum")]
    public double MaxCredit { get; set; } = 0;

    [Column("code_devise")]
    [Display(Name = "Devise")]
    public int CodeDevise { get; set; } = 1;

    [Column("company_id_client")]
    public int? CompanyId { get; set; }

    [MaxLength(100)]
    [Column("responsable_client")]
    [Display(Name = "Responsable")]
    public string? Responsable { get; set; }

    // Navigation
    [ForeignKey(nameof(CodeDevise))]
    public Devise? Devise { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public Company? Company { get; set; }

    public ICollection<FactureClient> FacturesClient { get; set; } = [];
    public ICollection<BonLivraison> BonsLivraison { get; set; } = [];
    public ICollection<CommandeVente> CommandesVente { get; set; } = [];
    public ICollection<DevisClient> DevisClient { get; set; } = [];
}
