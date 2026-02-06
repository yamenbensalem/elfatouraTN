using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité TvaProduit - Taux de TVA
/// </summary>
public class TvaProduit : BaseEntity
{
    public int CodeTVA { get; set; }
    public string Designation { get; set; } = string.Empty;
    public decimal Taux { get; set; }
    public bool ParDefaut { get; set; }
}
