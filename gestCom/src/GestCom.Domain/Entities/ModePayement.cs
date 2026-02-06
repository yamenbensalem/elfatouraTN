using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité ModePayement - Mode de paiement
/// </summary>
public class ModePayement : BaseEntity
{
    public string CodeMode { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Actif { get; set; } = true;
    
    // Alias for Application layer compatibility
    public string LibelleMode => Designation;
}
