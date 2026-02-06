using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité CommandeVente - Commande de vente
/// </summary>
public class CommandeVente : BaseEntity, IHasEntreprise
{
    public string NumeroCommande { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public string CodeClient { get; set; } = string.Empty;
    public DateTime DateCommande { get; set; }
    public DateTime? DateLivraison { get; set; }
    public DateTime? DateLivraisonPrevue { get; set; }
    public string? AdresseLivraison { get; set; }
    public decimal Remise { get; set; }
    public decimal TauxRemise { get; set; }
    public decimal TauxRemiseGlobale { get; set; }
    public decimal MontantRemise { get; set; }
    public string? CodeDevise { get; set; }
    public decimal TauxChange { get; set; } = 1;
    public string? Observations { get; set; }
    public string? Observation { get; set; }
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }
    public string Statut { get; set; } = "En cours"; // En cours, Validée, Livrée, Annulée
    public string? Notes { get; set; }
    public string? NumeroDevis { get; set; } // Référence au devis d'origine
    public string? NumeroBonLivraison { get; set; } // Référence au bon de livraison associé

    // Navigation properties
    public Entreprise? Entreprise { get; set; }
    public Client? Client { get; set; }
    public DevisClient? Devis { get; set; }
    public ICollection<LigneCommandeVente> Lignes { get; set; } = new List<LigneCommandeVente>();
    public ICollection<LigneCommandeVente> LignesCommande { get; set; } = new List<LigneCommandeVente>();
    public ICollection<BonLivraison> BonsLivraison { get; set; } = new List<BonLivraison>();
}
