namespace GestCom.Domain.Common;

/// <summary>
/// Classe de base pour toutes les entités du domaine
/// </summary>
public abstract class BaseEntity : IAuditable
{
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DateModification { get; set; }
    public string? UtilisateurCreation { get; set; }
    public string? UtilisateurModification { get; set; }
}
