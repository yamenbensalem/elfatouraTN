using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_GestCom.Data.Models;

[Table("commandevente")]
public class CommandeVente : ITenantOwned
{
    [Key]
    [Column("numero_commandevente")]
    [MaxLength(20)]
    public string NumeroCommandeVente { get; set; } = string.Empty;

    [Required]
    [Column("date_commandevente")]
    [Display(Name = "Date")]
    public DateTime DateCommandeVente { get; set; } = DateTime.Today;

    [Required, MaxLength(20)]
    [Column("code_client")]
    [Display(Name = "Client")]
    public string CodeClient { get; set; } = string.Empty;

    [Column("company_id_commandevente")]
    public int? CompanyId { get; set; }

    [Column("montantHT_commandevente")]
    [Display(Name = "Montant HT")]
    public double MontantHT { get; set; }

    [Column("montantTVA_commandevente")]
    [Display(Name = "TVA")]
    public double MontantTVA { get; set; }

    [Column("montantTTC_commandevente")]
    [Display(Name = "Montant TTC")]
    public double MontantTTC { get; set; }

    [Column("remise_commandevente")]
    [Display(Name = "Remise (%)")]
    public double Remise { get; set; }

    [Column("timbre_commandevente")]
    [Display(Name = "Timbre Fiscal")]
    public double Timbre { get; set; }

    [MaxLength(20)]
    [Column("etat_commandevente")]
    [Display(Name = "État")]
    public string EtatCommandeVente { get; set; } = "Ouvert";

    [MaxLength(20)]
    [Column("etat_livraison_commandevente")]
    [Display(Name = "État Livraison")]
    public string EtatLivraison { get; set; } = "Non Livré";

    [MaxLength(500)]
    [Column("note_commandevente")]
    [Display(Name = "Note")]
    public string? Note { get; set; }

    // Navigation
    [ForeignKey(nameof(CodeClient))]
    public Client? Client { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public Company? Company { get; set; }

    public ICollection<LigneCommandeVente> Lignes { get; set; } = [];
    public ICollection<BonLivraison> BonsLivraison { get; set; } = [];
}

[Table("lignecommandevente")]
public class LigneCommandeVente
{
    [Key]
    [Column("id_lignecommandvente")]
    public int Id { get; set; }

    [Required, MaxLength(20)]
    [Column("numero_commandevente")]
    public string NumeroCommandeVente { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    [Column("code_produit")]
    public string CodeProduit { get; set; } = string.Empty;

    [Column("quantite_lignecommandvente")]
    [Display(Name = "Quantité")]
    public double Quantite { get; set; }

    [Column("prixunitaire_lignecommandvente")]
    [Display(Name = "Prix Unitaire")]
    public double PrixUnitaire { get; set; }

    [Column("remise_lignecommandvente")]
    [Display(Name = "Remise (%)")]
    public double Remise { get; set; }

    [Column("tva_lignecommandvente")]
    [Display(Name = "TVA (%)")]
    public double Tva { get; set; }

    [Column("montantHT_lignecommandvente")]
    [Display(Name = "Montant HT")]
    public double MontantHT { get; set; }

    // Navigation
    [ForeignKey(nameof(NumeroCommandeVente))]
    public CommandeVente? CommandeVente { get; set; }

    [ForeignKey(nameof(CodeProduit))]
    public Produit? Produit { get; set; }
}
