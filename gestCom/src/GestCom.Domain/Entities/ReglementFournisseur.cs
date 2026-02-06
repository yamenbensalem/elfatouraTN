using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité ReglementFournisseur - Règlement fournisseur
/// </summary>
public class ReglementFournisseur : BaseEntity, IHasEntreprise
{
    public int Id { get; set; }
    public string CodeEntreprise { get; set; } = string.Empty;
    public string NumeroFacture { get; set; } = string.Empty;
    public string CodeFournisseur { get; set; } = string.Empty;
    public DateTime DateReglement { get; set; }
    public decimal Montant { get; set; }
    public string ModePayement { get; set; } = string.Empty;
    public string? NumeroTransaction { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Entreprise? Entreprise { get; set; }
    public FactureFournisseur? FactureFournisseur { get; set; }
    public Fournisseur? Fournisseur { get; set; }
}
