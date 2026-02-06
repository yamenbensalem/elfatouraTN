using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité Fournisseur - Gestion des fournisseurs
/// </summary>
public class Fournisseur : BaseEntity, IHasEntreprise
{
    public string CodeFournisseur { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public string MatriculeFiscale { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string RaisonSociale => Nom; // Alias pour compatibilité
    public string NomFournisseur => Nom; // Alias pour compatibilité
    public string TypePersonne { get; set; } = string.Empty;
    public string? TypeEntreprise { get; set; }
    public string? RIB { get; set; }
    
    // Adresse
    public string Adresse { get; set; } = string.Empty;
    public string? CodePostal { get; set; }
    public string Ville { get; set; } = string.Empty;
    public string Pays { get; set; } = "Tunisie";
    
    // Contact
    public string? Tel { get; set; }
    public string? Telephone => Tel; // Alias pour compatibilité
    public string? TelMobile { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? SiteWeb { get; set; }
    
    // Gestion
    public string Etat { get; set; } = "Actif";
    public int NombreTransactions { get; set; }
    public string? Note { get; set; }
    public bool Etranger { get; set; }
    public bool SoumisRAS { get; set; }
    public int CodeDevise { get; set; }
    public string? Responsable { get; set; }

    // Navigation properties
    public Entreprise? Entreprise { get; set; }
    public Devise? Devise { get; set; }
    public ICollection<DemandePrix> DemandesPrix { get; set; } = new List<DemandePrix>();
    public ICollection<CommandeAchat> Commandes { get; set; } = new List<CommandeAchat>();
    public ICollection<BonReception> BonsReception { get; set; } = new List<BonReception>();
    public ICollection<FactureFournisseur> Factures { get; set; } = new List<FactureFournisseur>();
    public ICollection<ReglementFournisseur> Reglements { get; set; } = new List<ReglementFournisseur>();
    public ICollection<Produit> Produits { get; set; } = new List<Produit>();
}
