using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité BonReception - Bon de réception
/// </summary>
public class BonReception : BaseEntity, IHasEntreprise
{
    public string NumeroBon { get; set; } = string.Empty;
    public string NumeroBonReception => NumeroBon; // Alias pour compatibilité
    public string CodeEntreprise { get; set; } = string.Empty;
    public string CodeFournisseur { get; set; } = string.Empty;
    public DateTime DateReception { get; set; }
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }
    public string Statut { get; set; } = "En cours"; // En cours, Réceptionné, Facturé
    public string? Notes { get; set; }
    public string? NumeroCommande { get; set; }
    public string? NumeroFacture { get; set; }

    // Navigation properties
    public Entreprise? Entreprise { get; set; }
    public Fournisseur? Fournisseur { get; set; }
    public CommandeAchat? CommandeAchat { get; set; }
    public ICollection<LigneBonReception> Lignes { get; set; } = new List<LigneBonReception>();
    public ICollection<LigneBonReception> LignesBonReception { get; set; } = new List<LigneBonReception>();
}
