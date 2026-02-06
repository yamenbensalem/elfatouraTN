using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité DemandePrix - Demande de prix/RFQ
/// </summary>
public class DemandePrix : BaseEntity, IHasEntreprise
{
    public string NumeroDemande { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public string CodeFournisseur { get; set; } = string.Empty;
    public DateTime DateDemande { get; set; }
    public DateTime? DateEcheance { get; set; }
    public string Statut { get; set; } = "Envoyée"; // Envoyée, Reçue, Acceptée, Refusée
    public string? Notes { get; set; }

    // Navigation properties
    public Entreprise? Entreprise { get; set; }
    public Fournisseur? Fournisseur { get; set; }
    public ICollection<LigneDemandePrix> Lignes { get; set; } = new List<LigneDemandePrix>();
}
