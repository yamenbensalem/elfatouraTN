using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité CategorieProduit - Catégorie de produit
/// </summary>
public class CategorieProduit : BaseEntity, IHasEntreprise
{
    public int CodeCategorie { get; set; }
    public string CodeEntreprise { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string Libelle => Designation; // Alias pour compatibilité
    public string LibelleCategorie => Designation; // Alias pour compatibilité
    public string? Description { get; set; }
    public int? CategorieParentId { get; set; }

    // Navigation properties
    public Entreprise? Entreprise { get; set; }
    public CategorieProduit? CategorieParent { get; set; }
    public ICollection<CategorieProduit> SousCategories { get; set; } = new List<CategorieProduit>();
    public ICollection<Produit> Produits { get; set; } = new List<Produit>();
}
