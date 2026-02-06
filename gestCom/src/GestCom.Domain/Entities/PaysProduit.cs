using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité PaysProduit - Pays d'origine
/// </summary>
public class PaysProduit : BaseEntity
{
    public int CodePays { get; set; }
    public string Designation { get; set; } = string.Empty;
    public string CodeISO { get; set; } = string.Empty;
}
