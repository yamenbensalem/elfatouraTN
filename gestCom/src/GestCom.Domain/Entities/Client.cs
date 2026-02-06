using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité Client - Gestion des clients
/// </summary>
public class Client : BaseEntity, IHasEntreprise
{
    public string CodeClient { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public string MatriculeFiscale { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string RaisonSociale => Nom; // Alias pour compatibilité
    public string NomClient => Nom; // Alias pour compatibilité
    public string TypePersonne { get; set; } = string.Empty; // Personne Physique / Personne Morale
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
    
    // Gestion commerciale
    public string Etat { get; set; } = "Actif"; // Actif / Inactif / Bloqué
    public int NombreTransactions { get; set; }
    public string? Note { get; set; }
    public bool Etranger { get; set; }
    public bool Exonore { get; set; }
    public decimal MaxCredit { get; set; }
    public decimal CreditMaximum { get; set; }
    public bool SoumisRAS { get; set; }
    public int CodeDevise { get; set; }
    public string? Responsable { get; set; }

    // Navigation properties
    public Entreprise? Entreprise { get; set; }
    public Devise? Devise { get; set; }
    public ICollection<DevisClient> Devis { get; set; } = new List<DevisClient>();
    public ICollection<CommandeVente> Commandes { get; set; } = new List<CommandeVente>();
    public ICollection<BonLivraison> BonsLivraison { get; set; } = new List<BonLivraison>();
    public ICollection<FactureClient> Factures { get; set; } = new List<FactureClient>();
    public ICollection<ReglementFacture> Reglements { get; set; } = new List<ReglementFacture>();
}
