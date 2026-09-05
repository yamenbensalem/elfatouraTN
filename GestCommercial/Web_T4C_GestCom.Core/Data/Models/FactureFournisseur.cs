using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_T4C_GestCom.Data.Models;

[Table("facturefournisseur")]
public class FactureFournisseur : ITenantOwned
{
    [Key]
    [Column("numero_facturefournisseur")]
    [MaxLength(20)]
    public string NumeroFactureFournisseur { get; set; } = string.Empty;

    [Required]
    [Column("date_facturefournisseur")]
    [Display(Name = "Date")]
    public DateTime DateFactureFournisseur { get; set; } = DateTime.Today;

    [Required, MaxLength(20)]
    [Column("code_fournisseur")]
    [Display(Name = "Fournisseur")]
    public string CodeFournisseur { get; set; } = string.Empty;

    [Column("company_id_facturefournisseur")]
    public int? CompanyId { get; set; }

    [Column("montantHT_facturefournisseur")]
    [Display(Name = "Montant HT")]
    public double MontantHT { get; set; }

    [Column("montantTVA_facturefournisseur")]
    [Display(Name = "TVA")]
    public double MontantTVA { get; set; }

    [Column("montantTTC_facturefournisseur")]
    [Display(Name = "Montant TTC")]
    public double MontantTTC { get; set; }

    [Column("timbre_facturefournisseur")]
    [Display(Name = "Timbre Fiscal")]
    public double Timbre { get; set; }

    [Column("retenue_facturefournisseur")]
    [Display(Name = "Retenue à la Source")]
    public double MontantRetenue { get; set; }

    [MaxLength(20)]
    [Column("etat_facturefournisseur")]
    [Display(Name = "État")]
    public string EtatFacture { get; set; } = "Facture Ouverte";

    [MaxLength(20)]
    [Column("etat_reglement_facturefournisseur")]
    [Display(Name = "État Règlement")]
    public string EtatReglement { get; set; } = "Non Réglé";

    [MaxLength(500)]
    [Column("note_facturefournisseur")]
    [Display(Name = "Note")]
    public string? Note { get; set; }

    // Navigation
    [ForeignKey(nameof(CodeFournisseur))]
    public Fournisseur? Fournisseur { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public Company? Company { get; set; }

    public ICollection<LigneFactureFournisseur> Lignes { get; set; } = [];
    public ICollection<ReglementFactureFournisseur> Reglements { get; set; } = [];
}

[Table("lignefacturefournisseur")]
public class LigneFactureFournisseur
{
    [Key]
    [Column("id_lignefacturefournisseur")]
    public int Id { get; set; }

    [Required, MaxLength(20)]
    [Column("numero_facturefournisseur")]
    public string NumeroFactureFournisseur { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    [Column("code_produit")]
    public string CodeProduit { get; set; } = string.Empty;

    [Column("quantite_lignefacturefournisseur")]
    [Display(Name = "Quantité")]
    public double Quantite { get; set; }

    [Column("prixunitaire_lignefacturefournisseur")]
    [Display(Name = "Prix Unitaire")]
    public double PrixUnitaire { get; set; }

    [Column("tva_lignefacturefournisseur")]
    [Display(Name = "TVA (%)")]
    public double Tva { get; set; }

    [Column("montantHT_lignefacturefournisseur")]
    [Display(Name = "Montant HT")]
    public double MontantHT { get; set; }

    // Navigation
    [ForeignKey(nameof(NumeroFactureFournisseur))]
    public FactureFournisseur? FactureFournisseur { get; set; }

    [ForeignKey(nameof(CodeProduit))]
    public Produit? Produit { get; set; }
}

[Table("reglementfacturefournisseur")]
public class ReglementFactureFournisseur
{
    [Key]
    [Column("id_reglementfacturefournisseur")]
    public int Id { get; set; }

    [Required, MaxLength(20)]
    [Column("numero_facturefournisseur")]
    public string NumeroFactureFournisseur { get; set; } = string.Empty;

    [Required]
    [Column("date_reglement")]
    [Display(Name = "Date Règlement")]
    public DateTime DateReglement { get; set; } = DateTime.Today;

    [Column("montant_reglement")]
    [Display(Name = "Montant")]
    public double Montant { get; set; }

    [Column("code_modepayement")]
    [Display(Name = "Mode de Paiement")]
    public int CodeModePayement { get; set; } = 1;

    [MaxLength(100)]
    [Column("reference_reglement")]
    [Display(Name = "Référence")]
    public string? Reference { get; set; }

    // Navigation
    [ForeignKey(nameof(NumeroFactureFournisseur))]
    public FactureFournisseur? FactureFournisseur { get; set; }

    [ForeignKey(nameof(CodeModePayement))]
    public ModePayement? ModePayement { get; set; }
}
