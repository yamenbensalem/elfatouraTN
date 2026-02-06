using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité PathLogo - Configuration des chemins de logos
/// </summary>
public class PathLogo : BaseEntity
{
    public int Id { get; set; }
    public string CheminLogo { get; set; } = string.Empty;
    public string? CheminRapport { get; set; }
}
