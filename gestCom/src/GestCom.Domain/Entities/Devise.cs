using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité Devise - Currency
/// </summary>
public class Devise : BaseEntity
{
    public int CodeDevise { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Symbole { get; set; } = string.Empty;
    public string CodeISO { get; set; } = string.Empty; // TND, EUR, USD
    public decimal TauxChange { get; set; } = 1;
    public bool DevisePrincipale { get; set; }
    
    // Alias for Application layer compatibility
    public string LibelleDevise => Nom;
}
