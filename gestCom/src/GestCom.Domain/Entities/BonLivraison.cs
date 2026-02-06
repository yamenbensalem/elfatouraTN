using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité BonLivraison - Bon de livraison
/// </summary>
public class BonLivraison : BaseEntity, IHasEntreprise
{
    public string NumeroBon { get; set; } = string.Empty;
    public string NumeroBonLivraison { get => NumeroBon; set => NumeroBon = value; } // Alias settable pour compatibilité
    public string CodeEntreprise { get; set; } = string.Empty;
    public string CodeClient { get; set; } = string.Empty;
    public DateTime DateLivraison { get; set; }
    public DateTime DateBonLivraison { get => DateLivraison; set => DateLivraison = value; } // Alias settable pour compatibilité
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }
    public string Statut { get; set; } = "En cours"; // En cours, Livré, Facturé
    public string? Notes { get; set; }
    public string? Observations { get; set; }
    public string? AdresseLivraison { get; set; }
    public string? NumeroCommande { get; set; }
    public string? NumeroFacture { get; set; }
    public bool Facture { get; set; } // Indique si un BL a été facturé

    // Navigation properties
    public Entreprise? Entreprise { get; set; }
    public Client? Client { get; set; }
    public CommandeVente? CommandeVente { get; set; }
    public ICollection<LigneBonLivraison> Lignes { get; set; } = new List<LigneBonLivraison>();
    public ICollection<LigneBonLivraison> LignesBonLivraison { get; set; } = new List<LigneBonLivraison>();
    public ICollection<BonLivraison_Facture> FacturesLiees { get; set; } = new List<BonLivraison_Facture>();
}
