using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité FabriquantProduit - Fabricant/Manufacturer
/// </summary>
public class FabriquantProduit : BaseEntity
{
    public int CodeFabriquant { get; set; }
    public string Designation { get; set; } = string.Empty;
    public string? Pays { get; set; }
    public string? SiteWeb { get; set; }
}
