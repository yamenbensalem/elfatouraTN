using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité DouaneProduit - Code douanier
/// </summary>
public class DouaneProduit : BaseEntity
{
    public int CodeDouane { get; set; }
    public string Designation { get; set; } = string.Empty;
    public string? NumeroHS { get; set; } // Harmonized System code
    public string? Description { get; set; }
}
