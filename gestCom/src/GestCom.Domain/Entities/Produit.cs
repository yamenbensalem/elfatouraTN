using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité Produit - Gestion des produits
/// </summary>
public class Produit : BaseEntity, IHasEntreprise
{
    public string CodeProduit { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string? CodeBarre { get; set; }
    public string? Reference { get; set; }
    
    // Prix et gestion financière
    public decimal PrixUnitaire { get; set; }
    public int CodeDevise { get; set; }
    public decimal PrixAchatTTC { get; set; }
    public decimal TauxMarge { get; set; }
    public decimal PrixVenteHT { get; set; }
    public decimal Remise { get; set; }
    public decimal PrixVenteTTC { get; set; }
    public decimal Fodec { get; set; }
    public decimal TauxTVA { get; set; }
    public decimal TauxFODEC { get; set; }
    public string? CodeUnite { get; set; }
    public int CodeTVA { get; set; }
    
    // Stock
    public decimal Quantite { get; set; }
    public decimal StockMinimal { get; set; }
    public decimal RemiseMaximale { get; set; }
    public string? Rayon { get; set; }
    public string? Etage { get; set; }
    
    // Relations avec autres référentiels
    public string? CodeFournisseur { get; set; }
    public int? CodeUniteProduit { get; set; }
    public int? CodeTVAProduit { get; set; }
    public int? CodeCategorieProduit { get; set; }
    public string? CodeCategorie { get; set; } // Alias string pour compatibilité
    public int? CodeMagasinProduit { get; set; }
    public string? CodeMagasin { get; set; } // Alias string pour compatibilité
    public int? CodeFabriquantProduit { get; set; }
    public int? CodePaysProduit { get; set; }
    public int? CodeDouaneProduit { get; set; }

    // Navigation properties
    public Entreprise? Entreprise { get; set; }
    public Devise? Devise { get; set; }
    public Fournisseur? Fournisseur { get; set; }
    public UniteProduit? UniteProduit { get; set; }
    public TvaProduit? TvaProduit { get; set; }
    public CategorieProduit? CategorieProduit { get; set; }
    public MagasinProduit? MagasinProduit { get; set; }
    public FabriquantProduit? FabriquantProduit { get; set; }
    public PaysProduit? PaysProduit { get; set; }
    public DouaneProduit? DouaneProduit { get; set; }
    
    // Collections
    public ICollection<LigneDevisClient> LignesDevis { get; set; } = new List<LigneDevisClient>();
    public ICollection<LigneCommandeVente> LignesCommandeVente { get; set; } = new List<LigneCommandeVente>();
    public ICollection<LigneCommandeAchat> LignesCommandeAchat { get; set; } = new List<LigneCommandeAchat>();
    public ICollection<LigneBonLivraison> LignesBonLivraison { get; set; } = new List<LigneBonLivraison>();
    public ICollection<LigneBonReception> LignesBonReception { get; set; } = new List<LigneBonReception>();
    public ICollection<LigneFactureClient> LignesFactureClient { get; set; } = new List<LigneFactureClient>();
    public ICollection<LigneFactureFournisseur> LignesFactureFournisseur { get; set; } = new List<LigneFactureFournisseur>();
}
