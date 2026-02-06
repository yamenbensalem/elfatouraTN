using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité UniteProduit - Unité de mesure
/// </summary>
public class UniteProduit : BaseEntity
{
    public int CodeUnite { get; set; }
    public string Designation { get; set; } = string.Empty;
    public string Libelle => Designation; // Alias pour compatibilité
    public string Symbole { get; set; } = string.Empty; // pcs, kg, m, l, etc.
}
