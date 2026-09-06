using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_GestCom.Data.Models;

[Table("devisClient")]
public class DevisClient : ITenantOwned
{
    [Key]
    [Column("numero_devis")]
    [MaxLength(20)]
    public string NumeroDevis { get; set; } = string.Empty;

    [Required]
    [Column("date_devis")]
    [Display(Name = "Date")]
    public DateTime DateDevis { get; set; } = DateTime.Today;

    [Required, MaxLength(20)]
    [Column("code_client")]
    [Display(Name = "Client")]
    public string CodeClient { get; set; } = string.Empty;

    [Column("company_id_devis")]
    public int? CompanyId { get; set; }

    [Column("montantHT_devis")]
    [Display(Name = "Montant HT")]
    public double MontantHT { get; set; }

    [Column("montantTVA_devis")]
    [Display(Name = "TVA")]
    public double MontantTVA { get; set; }

    [Column("montantTTC_devis")]
    [Display(Name = "Montant TTC")]
    public double MontantTTC { get; set; }

    [Column("remise_devis")]
    [Display(Name = "Remise (%)")]
    public double Remise { get; set; }

    [Column("timbre_devis")]
    [Display(Name = "Timbre Fiscal")]
    public double Timbre { get; set; }

    [MaxLength(20)]
    [Column("etat_devis")]
    [Display(Name = "État")]
    public string EtatDevis { get; set; } = "Ouvert";

    [MaxLength(500)]
    [Column("note_devis")]
    [Display(Name = "Note")]
    public string? Note { get; set; }

    // Navigation
    [ForeignKey(nameof(CodeClient))]
    public Client? Client { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public Company? Company { get; set; }

    public ICollection<LigneDevisClient> Lignes { get; set; } = [];
}

[Table("ligneDevisClient")]
public class LigneDevisClient
{
    [Key]
    [Column("id_lignedevis")]
    public int Id { get; set; }

    [Required, MaxLength(20)]
    [Column("numero_devis")]
    public string NumeroDevis { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    [Column("code_produit")]
    public string CodeProduit { get; set; } = string.Empty;

    [Column("quantite_lignedevis")]
    [Display(Name = "Quantité")]
    public double Quantite { get; set; }

    [Column("prixunitaire_lignedevis")]
    [Display(Name = "Prix Unitaire")]
    public double PrixUnitaire { get; set; }

    [Column("remise_lignedevis")]
    [Display(Name = "Remise (%)")]
    public double Remise { get; set; }

    [Column("tva_lignedevis")]
    [Display(Name = "TVA (%)")]
    public double Tva { get; set; }

    [Column("montantHT_lignedevis")]
    [Display(Name = "Montant HT")]
    public double MontantHT { get; set; }

    // Navigation
    [ForeignKey(nameof(NumeroDevis))]
    public DevisClient? DevisClient { get; set; }

    [ForeignKey(nameof(CodeProduit))]
    public Produit? Produit { get; set; }
}
