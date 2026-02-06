using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité RetenuSource - Retenue à la source (Tax withholding)
/// </summary>
public class RetenuSource : BaseEntity
{
    public int CodeRetenu { get; set; }
    public string Designation { get; set; } = string.Empty;
    public decimal Taux { get; set; }
    public string? Description { get; set; }
}
