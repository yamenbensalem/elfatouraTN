using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité FactureFournisseur - Facture fournisseur
/// </summary>
public class FactureFournisseur : BaseEntity, IHasEntreprise
{
    public string NumeroFacture { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public string CodeFournisseur { get; set; } = string.Empty;
    public DateTime DateFacture { get; set; }
    public DateTime? DateEcheance { get; set; }
    public decimal Remise { get; set; }
    public decimal TauxRemiseGlobale { get; set; }
    public decimal MontantRemise { get; set; }
    public string? NumeroBonReception { get; set; }
    public string? Observation { get; set; }
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal Timbre { get; set; }
    public decimal MontantTTC { get; set; }
    public decimal APayer { get; set; }
    public decimal MontantRegle { get; set; }
    public decimal MontantRestant { get; set; }
    public decimal MontantFodec { get; set; }
    public decimal TauxRAS { get; set; }
    public decimal MontantRAS { get; set; }
    public decimal NetAPayer { get; set; }
    public string Statut { get; set; } = "Brouillon"; // Brouillon, Validée, Payée, Annulée
    public string? ModePayement { get; set; }
    public string? Notes { get; set; }
    public string? NumeroFactureFournisseur { get; set; } // Numéro de facture du fournisseur

    // Navigation properties
    public Entreprise? Entreprise { get; set; }
    public Fournisseur? Fournisseur { get; set; }
    public ModePayement? ModePayementNavigation { get; set; }
    public ICollection<LigneFactureFournisseur> Lignes { get; set; } = new List<LigneFactureFournisseur>();
    public ICollection<LigneFactureFournisseur> LignesFacture { get; set; } = new List<LigneFactureFournisseur>();
    public ICollection<ReglementFournisseur> Reglements { get; set; } = new List<ReglementFournisseur>();
}
