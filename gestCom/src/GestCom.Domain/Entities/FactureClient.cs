using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité FactureClient - Facture client
/// </summary>
public class FactureClient : BaseEntity, IHasEntreprise
{
    public string NumeroFacture { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public string CodeClient { get; set; } = string.Empty;
    public DateTime DateFacture { get; set; }
    public DateTime? DateEcheance { get; set; }
    public decimal Remise { get; set; }
    public decimal TauxRemise { get; set; }
    public decimal TauxRemiseGlobale { get; set; }
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal Timbre { get; set; }
    public decimal MontantTTC { get; set; }
    public decimal APayer { get; set; }
    public decimal MontantApresRAS { get; set; } // Retenue à la Source
    public decimal MontantRestant { get; set; }
    public decimal MontantRegle { get; set; } // Montant déjà payé
    public decimal MontantFodec { get; set; }
    public decimal MontantFODEC { get => MontantFodec; set => MontantFodec = value; } // Alias
    public decimal MontantRemise { get; set; }
    public decimal TauxRAS { get; set; }
    public decimal MontantRAS { get; set; }
    public decimal NetAPayer { get; set; }
    public string Origine { get; set; } = "Directe"; // Directe, CommandeVente, BonLivraison
    public string Statut { get; set; } = "Brouillon"; // Brouillon, Validée, Payée, Annulée
    public string? ModePayement { get; set; }
    public string? CodeModePaiement { get; set; }
    public string? CodeDevise { get; set; }
    public decimal TauxChange { get; set; } = 1;
    public string? NumeroBonLivraison { get; set; }
    public string? NumeroCommande { get; set; }
    public string? Observations { get; set; }
    public string? Observation { get; set; }
    public bool Avoir { get; set; } // Facture d'avoir (crédit note)
    public string? Notes { get; set; }

    // Navigation properties
    public Entreprise? Entreprise { get; set; }
    public Client? Client { get; set; }
    public ModePayement? ModePayementNavigation { get; set; }
    public Devise? DeviseNavigation { get; set; }
    public ICollection<LigneFactureClient> Lignes { get; set; } = new List<LigneFactureClient>();
    public ICollection<LigneFactureClient> LignesFacture { get; set; } = new List<LigneFactureClient>();
    public ICollection<ReglementFacture> Reglements { get; set; } = new List<ReglementFacture>();
    public ICollection<BonLivraison_Facture> BonsLivraisonLies { get; set; } = new List<BonLivraison_Facture>();
}
