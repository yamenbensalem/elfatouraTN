using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité MagasinProduit - Magasin/Entrepôt
/// </summary>
public class MagasinProduit : BaseEntity, IHasEntreprise
{
    public int CodeMagasin { get; set; }
    public string CodeEntreprise { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string Libelle => Designation; // Alias pour compatibilité
    public string? Adresse { get; set; }
    public string? Ville { get; set; }
    public string? Responsable { get; set; }
    public bool Principal { get; set; }

    // Navigation properties
    public Entreprise? Entreprise { get; set; }
    public ICollection<Produit> Produits { get; set; } = new List<Produit>();
}
