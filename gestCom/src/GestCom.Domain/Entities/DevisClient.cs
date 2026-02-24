using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité DevisClient - Devis/Quote pour client
/// </summary>
public class DevisClient : BaseEntity, IHasEntreprise
{
    public string NumeroDevis { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public string CodeClient { get; set; } = string.Empty;
    public DateTime DateDevis { get; set; }
    public DateTime? DateEcheance { get; set; }
    public DateTime? DateValidite { get; set; }
    public decimal Remise { get; set; }
    public decimal TauxRemise { get; set; }
    public decimal TauxRemiseGlobale { get; set; }
    public decimal MontantRemise { get; set; }
    public decimal Timbre { get; set; }
    public string? CodeDevise { get; set; }
    public decimal TauxChange { get; set; } = 1;
    public string? Observations { get; set; }
    public string? Observation { get => Observations; set => Observations = value; }
    public string? NumeroCommande { get; set; }
    public decimal MontantHT { get; set; }
    public decimal MontantTVA { get; set; }
    public decimal MontantTTC { get; set; }
    public string Statut { get; set; } = "Brouillon"; // Brouillon, Envoyé, Accepté, Refusé, Expiré
    public string? Notes { get; set; }

    // Navigation properties
    public Entreprise? Entreprise { get; set; }
    public Client? Client { get; set; }
    public ICollection<LigneDevisClient> Lignes { get; set; } = new List<LigneDevisClient>();
}
