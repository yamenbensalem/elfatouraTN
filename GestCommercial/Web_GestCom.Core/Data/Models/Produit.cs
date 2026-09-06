using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_GestCom.Data.Models;

[Table("produit")]
public class Produit : ITenantOwned
{
    [Key]
    [Column("code_produit")]
    [MaxLength(30)]
    public string CodeProduit { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    [Column("designation_produit")]
    [Display(Name = "Désignation")]
    public string DesignationProduit { get; set; } = string.Empty;

    [Column("prixunitaire_produit")]
    [Display(Name = "Prix Unitaire")]
    public double PrixUnitaire { get; set; }

    [Column("code_devise")]
    [Display(Name = "Devise")]
    public int CodeDevise { get; set; } = 1;

    [Column("quantite_produit")]
    [Display(Name = "Quantité en stock")]
    public double Quantite { get; set; }

    [MaxLength(20)]
    [Column("code_fournisseur")]
    [Display(Name = "Fournisseur")]
    public string? CodeFournisseur { get; set; }

    [Column("company_id_produit")]
    public int? CompanyId { get; set; }

    [Column("code_uniteproduit")]
    [Display(Name = "Unité")]
    public int CodeUniteProduit { get; set; } = 1;

    [Column("prixachatTTC_produit")]
    [Display(Name = "Prix Achat TTC")]
    public double PrixAchatTTC { get; set; }

    [Column("tauxmarge_produit")]
    [Display(Name = "Taux Marge (%)")]
    public double TauxMarge { get; set; }

    [Column("prixventeHT_produit")]
    [Display(Name = "Prix Vente HT")]
    public double PrixVenteHT { get; set; }

    [Column("remise_produit")]
    [Display(Name = "Remise (%)")]
    public double Remise { get; set; }

    [Column("code_tvaproduit")]
    [Display(Name = "TVA")]
    public int CodeTvaProduit { get; set; } = 1;

    [Column("fodec_produit")]
    [Display(Name = "FODEC (%)")]
    public double Fodec { get; set; }

    [Column("prixventeTTC_produit")]
    [Display(Name = "Prix Vente TTC")]
    public double PrixVenteTTC { get; set; }

    [Column("code_categorieproduit")]
    [Display(Name = "Catégorie")]
    public int CodeCategorieProduit { get; set; } = 1;

    [Column("code_fabriquantproduit")]
    [Display(Name = "Fabricant")]
    public int CodeFabriquantProduit { get; set; } = 1;

    [Column("stockminimal_produit")]
    [Display(Name = "Stock Minimal")]
    public double StockMinimal { get; set; }

    [Column("remisemaximale_produit")]
    [Display(Name = "Remise Maximale (%)")]
    public double RemiseMaximale { get; set; }

    [MaxLength(100)]
    [Column("rayon_produit")]
    [Display(Name = "Rayon")]
    public string? Rayon { get; set; }

    [MaxLength(100)]
    [Column("etage_produit")]
    [Display(Name = "Étage")]
    public string? Etage { get; set; }

    // Navigation
    [ForeignKey(nameof(CodeDevise))]
    public Devise? Devise { get; set; }

    [ForeignKey(nameof(CodeUniteProduit))]
    public UniteProduit? UniteProduit { get; set; }

    [ForeignKey(nameof(CodeTvaProduit))]
    public TvaProduit? TvaProduit { get; set; }

    [ForeignKey(nameof(CodeCategorieProduit))]
    public CategorieProduit? CategorieProduit { get; set; }

    [ForeignKey(nameof(CodeFabriquantProduit))]
    public FabriquantProduit? FabriquantProduit { get; set; }

    [ForeignKey(nameof(CodeFournisseur))]
    public Fournisseur? Fournisseur { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public Company? Company { get; set; }
}
