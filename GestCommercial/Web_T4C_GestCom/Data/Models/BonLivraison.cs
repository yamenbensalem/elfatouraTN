using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_T4C_GestCom.Data.Models;

[Table("bonlivraison")]
public class BonLivraison : ITenantOwned
{
    [Key]
    [Column("numero_bonlivraison")]
    [MaxLength(20)]
    public string NumeroBonLivraison { get; set; } = string.Empty;

    [Required]
    [Column("date_bonlivraison")]
    [Display(Name = "Date")]
    public DateTime DateBonLivraison { get; set; } = DateTime.Today;

    [Required, MaxLength(20)]
    [Column("code_client")]
    [Display(Name = "Client")]
    public string CodeClient { get; set; } = string.Empty;

    [Column("company_id_bonlivraison")]
    public int? CompanyId { get; set; }

    [MaxLength(20)]
    [Column("numero_commandevente")]
    [Display(Name = "Commande")]
    public string? NumeroCommandeVente { get; set; }

    [Column("montantHT_bonlivraison")]
    [Display(Name = "Montant HT")]
    public double MontantHT { get; set; }

    [Column("montantTVA_bonlivraison")]
    [Display(Name = "TVA")]
    public double MontantTVA { get; set; }

    [Column("montantTTC_bonlivraison")]
    [Display(Name = "Montant TTC")]
    public double MontantTTC { get; set; }

    [Column("remise_bonlivraison")]
    [Display(Name = "Remise (%)")]
    public double Remise { get; set; }

    [MaxLength(20)]
    [Column("etat_bonlivraison")]
    [Display(Name = "État")]
    public string EtatBonLivraison { get; set; } = "Ouvert";

    [MaxLength(20)]
    [Column("etat_facture_bonlivraison")]
    [Display(Name = "État Facturation")]
    public string EtatFacture { get; set; } = "Non Facturé";

    [MaxLength(500)]
    [Column("note_bonlivraison")]
    [Display(Name = "Note")]
    public string? Note { get; set; }

    // Navigation
    [ForeignKey(nameof(CodeClient))]
    public Client? Client { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public Company? Company { get; set; }

    [ForeignKey(nameof(NumeroCommandeVente))]
    public CommandeVente? CommandeVente { get; set; }

    public ICollection<LigneBonLivraison> Lignes { get; set; } = [];
}

[Table("lignebonlivraison")]
public class LigneBonLivraison
{
    [Key]
    [Column("id_lignebonlivraison")]
    public int Id { get; set; }

    [Required, MaxLength(20)]
    [Column("numero_bonlivraison")]
    public string NumeroBonLivraison { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    [Column("code_produit")]
    public string CodeProduit { get; set; } = string.Empty;

    [Column("quantite_lignebonlivraison")]
    [Display(Name = "Quantité")]
    public double Quantite { get; set; }

    [Column("prixunitaire_lignebonlivraison")]
    [Display(Name = "Prix Unitaire")]
    public double PrixUnitaire { get; set; }

    [Column("remise_lignebonlivraison")]
    [Display(Name = "Remise (%)")]
    public double Remise { get; set; }

    [Column("tva_lignebonlivraison")]
    [Display(Name = "TVA (%)")]
    public double Tva { get; set; }

    [Column("montantHT_lignebonlivraison")]
    [Display(Name = "Montant HT")]
    public double MontantHT { get; set; }

    // Navigation
    [ForeignKey(nameof(NumeroBonLivraison))]
    public BonLivraison? BonLivraison { get; set; }

    [ForeignKey(nameof(CodeProduit))]
    public Produit? Produit { get; set; }
}
