using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_T4C_GestCom.Data.Models;

[Table("bonreception")]
public class BonReception
{
    [Key]
    [Column("numero_bonreception")]
    [MaxLength(20)]
    public string NumeroBonReception { get; set; } = string.Empty;

    [Required]
    [Column("date_bonreception")]
    [Display(Name = "Date")]
    public DateTime DateBonReception { get; set; } = DateTime.Today;

    [Required, MaxLength(20)]
    [Column("code_fournisseur")]
    [Display(Name = "Fournisseur")]
    public string CodeFournisseur { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("numero_commandeachat")]
    [Display(Name = "Commande")]
    public string? NumeroCommandeAchat { get; set; }

    [Column("montantHT_bonreception")]
    [Display(Name = "Montant HT")]
    public double MontantHT { get; set; }

    [Column("montantTVA_bonreception")]
    [Display(Name = "TVA")]
    public double MontantTVA { get; set; }

    [Column("montantTTC_bonreception")]
    [Display(Name = "Montant TTC")]
    public double MontantTTC { get; set; }

    [MaxLength(20)]
    [Column("etat_bonreception")]
    [Display(Name = "État")]
    public string EtatBonReception { get; set; } = "Ouvert";

    [MaxLength(20)]
    [Column("etat_facture_bonreception")]
    [Display(Name = "État Facturation")]
    public string EtatFacture { get; set; } = "Non Facturé";

    [MaxLength(500)]
    [Column("note_bonreception")]
    [Display(Name = "Note")]
    public string? Note { get; set; }

    // Navigation
    [ForeignKey(nameof(CodeFournisseur))]
    public Fournisseur? Fournisseur { get; set; }

    [ForeignKey(nameof(NumeroCommandeAchat))]
    public CommandeAchat? CommandeAchat { get; set; }

    public ICollection<LigneBonReception> Lignes { get; set; } = [];
}

[Table("lignebonreception")]
public class LigneBonReception
{
    [Key]
    [Column("id_lignebonreception")]
    public int Id { get; set; }

    [Required, MaxLength(20)]
    [Column("numero_bonreception")]
    public string NumeroBonReception { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    [Column("code_produit")]
    public string CodeProduit { get; set; } = string.Empty;

    [Column("quantite_lignebonreception")]
    [Display(Name = "Quantité")]
    public double Quantite { get; set; }

    [Column("prixunitaire_lignebonreception")]
    [Display(Name = "Prix Unitaire")]
    public double PrixUnitaire { get; set; }

    [Column("tva_lignebonreception")]
    [Display(Name = "TVA (%)")]
    public double Tva { get; set; }

    [Column("montantHT_lignebonreception")]
    [Display(Name = "Montant HT")]
    public double MontantHT { get; set; }

    // Navigation
    [ForeignKey(nameof(NumeroBonReception))]
    public BonReception? BonReception { get; set; }

    [ForeignKey(nameof(CodeProduit))]
    public Produit? Produit { get; set; }
}
