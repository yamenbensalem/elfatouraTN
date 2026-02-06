using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité CommandeAchat - Commande d'achat/Purchase Order
/// </summary>
public class CommandeAchat : BaseEntity, IHasEntreprise
{
    public string NumeroCommande { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public string CodeFournisseur { get; set; } = string.Empty;
    public DateTime DateCommande { get; set; }
    public DateTime? DateLivraison { get; set; }
    public DateTime? DateLivraisonPrevue { get; set; }
    public string? NumeroDemandePrix { get; set; }
    public string? NumeroBonReception { get; set; }
    public decimal Remise { get; set; }
    public decimal TauxRemise { get; set; }
    public string? CodeDevise { get; set; }
    public decimal TauxChange { get; set; } = 1;
    public string? Observations { get; set; }
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }
    public string Statut { get; set; } = "En cours"; // En cours, Validée, Réceptionnée, Annulée
    public string? Notes { get; set; }

    // Navigation properties
    public Entreprise? Entreprise { get; set; }
    public Fournisseur? Fournisseur { get; set; }
    public ICollection<LigneCommandeAchat> Lignes { get; set; } = new List<LigneCommandeAchat>();
    public ICollection<LigneCommandeAchat> LignesCommande { get; set; } = new List<LigneCommandeAchat>();
    public ICollection<BonReception> BonsReception { get; set; } = new List<BonReception>();
}
