using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_T4C_GestCom.Data.Models;

[Table("factureclient")]
public class FactureClient : ITenantOwned
{
    [Key]
    [Column("numero_factureclient")]
    [MaxLength(20)]
    public string NumeroFactureClient { get; set; } = string.Empty;

    [Required]
    [Column("date_factureclient")]
    [Display(Name = "Date")]
    public DateTime DateFactureClient { get; set; } = DateTime.Today;

    [Required, MaxLength(20)]
    [Column("code_client")]
    [Display(Name = "Client")]
    public string CodeClient { get; set; } = string.Empty;

    [Column("company_id_factureclient")]
    public int? CompanyId { get; set; }

    [Column("montantHT_factureclient")]
    [Display(Name = "Montant HT")]
    public double MontantHT { get; set; }

    [Column("montantTVA_factureclient")]
    [Display(Name = "TVA")]
    public double MontantTVA { get; set; }

    [Column("montantTTC_factureclient")]
    [Display(Name = "Montant TTC")]
    public double MontantTTC { get; set; }

    [Column("remise_factureclient")]
    [Display(Name = "Remise (%)")]
    public double Remise { get; set; }

    [Column("timbre_factureclient")]
    [Display(Name = "Timbre Fiscal")]
    public double Timbre { get; set; }

    [Column("retenue_factureclient")]
    [Display(Name = "Retenue à la Source")]
    public double MontantRetenue { get; set; }

    [Column("fodec_factureclient")]
    [Display(Name = "FODEC")]
    public double Fodec { get; set; }

    [MaxLength(20)]
    [Column("etat_factureclient")]
    [Display(Name = "État")]
    public string EtatFacture { get; set; } = "Facture Ouverte";

    [MaxLength(20)]
    [Column("etat_reglement_factureclient")]
    [Display(Name = "État Règlement")]
    public string EtatReglement { get; set; } = "Non Réglé";

    [Column("avoir_factureclient")]
    [Display(Name = "Avoir")]
    public bool IsAvoir { get; set; } = false;

    [MaxLength(500)]
    [Column("note_factureclient")]
    [Display(Name = "Note")]
    public string? Note { get; set; }

    // Navigation
    [ForeignKey(nameof(CodeClient))]
    public Client? Client { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public Company? Company { get; set; }

    public ICollection<LigneFactureClient> Lignes { get; set; } = [];
    public ICollection<ReglementFactureClient> Reglements { get; set; } = [];
}

[Table("lignefactureclient")]
public class LigneFactureClient
{
    [Key]
    [Column("id_lignefactureclient")]
    public int Id { get; set; }

    [Required, MaxLength(20)]
    [Column("numero_factureclient")]
    public string NumeroFactureClient { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    [Column("code_produit")]
    public string CodeProduit { get; set; } = string.Empty;

    [Column("quantite_lignefactureclient")]
    [Display(Name = "Quantité")]
    public double Quantite { get; set; }

    [Column("prixunitaire_lignefactureclient")]
    [Display(Name = "Prix Unitaire")]
    public double PrixUnitaire { get; set; }

    [Column("remise_lignefactureclient")]
    [Display(Name = "Remise (%)")]
    public double Remise { get; set; }

    [Column("tva_lignefactureclient")]
    [Display(Name = "TVA (%)")]
    public double Tva { get; set; }

    [Column("fodec_lignefactureclient")]
    [Display(Name = "FODEC (%)")]
    public double Fodec { get; set; }

    [Column("montantHT_lignefactureclient")]
    [Display(Name = "Montant HT")]
    public double MontantHT { get; set; }

    // Navigation
    [ForeignKey(nameof(NumeroFactureClient))]
    public FactureClient? FactureClient { get; set; }

    [ForeignKey(nameof(CodeProduit))]
    public Produit? Produit { get; set; }
}

[Table("reglementfactureclient")]
public class ReglementFactureClient
{
    [Key]
    [Column("id_reglementfactureclient")]
    public int Id { get; set; }

    [Required, MaxLength(20)]
    [Column("numero_factureclient")]
    public string NumeroFactureClient { get; set; } = string.Empty;

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

    [MaxLength(500)]
    [Column("note_reglement")]
    [Display(Name = "Note")]
    public string? Note { get; set; }

    // Navigation
    [ForeignKey(nameof(NumeroFactureClient))]
    public FactureClient? FactureClient { get; set; }

    [ForeignKey(nameof(CodeModePayement))]
    public ModePayement? ModePayement { get; set; }
}
