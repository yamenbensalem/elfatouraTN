namespace GestCom.Domain.Common;

/// <summary>
/// Interface pour les entités auditable (création et modification)
/// </summary>
public interface IAuditable
{
    DateTime DateCreation { get; set; }
    DateTime? DateModification { get; set; }
    string? UtilisateurCreation { get; set; }
    string? UtilisateurModification { get; set; }
}
