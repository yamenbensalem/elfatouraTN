namespace GestCom.Domain.Entities;

/// <summary>
/// Table de jointure entre BonLivraison et FactureClient
/// </summary>
public class BonLivraison_Facture
{
    public string NumeroBon { get; set; } = string.Empty;
    public string NumeroFacture { get; set; } = string.Empty;

    // Navigation properties
    public BonLivraison? BonLivraison { get; set; }
    public FactureClient? FactureClient { get; set; }
}
