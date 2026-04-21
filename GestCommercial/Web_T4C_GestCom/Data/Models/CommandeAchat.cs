using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_T4C_GestCom.Data.Models;

[Table("commandeachat")]
public class CommandeAchat : ITenantOwned
{
    [Key]
    [Column("numero_commandeachat")]
    [MaxLength(20)]
    public string NumeroCommandeAchat { get; set; } = string.Empty;

    [Required]
    [Column("date_commandeachat")]
    [Display(Name = "Date")]
    public DateTime DateCommandeAchat { get; set; } = DateTime.Today;

    [Required, MaxLength(20)]
    [Column("code_fournisseur")]
    [Display(Name = "Fournisseur")]
    public string CodeFournisseur { get; set; } = string.Empty;

    [Column("company_id_commandeachat")]
    public int? CompanyId { get; set; }

    [Column("montantHT_commandeachat")]
    [Display(Name = "Montant HT")]
    public double MontantHT { get; set; }

    [Column("montantTVA_commandeachat")]
    [Display(Name = "TVA")]
    public double MontantTVA { get; set; }

    [Column("montantTTC_commandeachat")]
    [Display(Name = "Montant TTC")]
    public double MontantTTC { get; set; }

    [MaxLength(20)]
    [Column("etat_commandeachat")]
    [Display(Name = "État")]
    public string EtatCommandeAchat { get; set; } = "Ouvert";

    [MaxLength(20)]
    [Column("etat_reception_commandeachat")]
    [Display(Name = "État Réception")]
    public string EtatReception { get; set; } = "Non Reçu";

    [MaxLength(500)]
    [Column("note_commandeachat")]
    [Display(Name = "Note")]
    public string? Note { get; set; }

    // Navigation
    [ForeignKey(nameof(CodeFournisseur))]
    public Fournisseur? Fournisseur { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public Company? Company { get; set; }

    public ICollection<LigneCommandeAchat> Lignes { get; set; } = [];
    public ICollection<BonReception> BonsReception { get; set; } = [];
}

[Table("lignecommandeachat")]
public class LigneCommandeAchat
{
    [Key]
    [Column("id_lignecommandeachat")]
    public int Id { get; set; }

    [Required, MaxLength(20)]
    [Column("numero_commandeachat")]
    public string NumeroCommandeAchat { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    [Column("code_produit")]
    public string CodeProduit { get; set; } = string.Empty;

    [Column("quantite_lignecommandeachat")]
    [Display(Name = "Quantité")]
    public double Quantite { get; set; }

    [Column("prixunitaire_lignecommandeachat")]
    [Display(Name = "Prix Unitaire")]
    public double PrixUnitaire { get; set; }

    [Column("tva_lignecommandeachat")]
    [Display(Name = "TVA (%)")]
    public double Tva { get; set; }

    [Column("montantHT_lignecommandeachat")]
    [Display(Name = "Montant HT")]
    public double MontantHT { get; set; }

    // Navigation
    [ForeignKey(nameof(NumeroCommandeAchat))]
    public CommandeAchat? CommandeAchat { get; set; }

    [ForeignKey(nameof(CodeProduit))]
    public Produit? Produit { get; set; }
}
