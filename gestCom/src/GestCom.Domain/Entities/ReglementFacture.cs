using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité ReglementFacture - Règlement de facture client
/// </summary>
public class ReglementFacture : BaseEntity, IHasEntreprise
{
    public int Id { get; set; }
    public string CodeEntreprise { get; set; } = string.Empty;
    public string NumeroFacture { get; set; } = string.Empty;
    public string CodeClient { get; set; } = string.Empty;
    public DateTime DateReglement { get; set; }
    public decimal Montant { get; set; }
    public string ModePayement { get; set; } = string.Empty; // Espèces, Chèque, Virement, Carte
    public string? NumeroTransaction { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Entreprise? Entreprise { get; set; }
    public FactureClient? FactureClient { get; set; }
    public Client? Client { get; set; }
}
