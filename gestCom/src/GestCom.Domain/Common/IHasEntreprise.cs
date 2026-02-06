namespace GestCom.Domain.Common;

/// <summary>
/// Interface pour les entités avec multi-tenancy (filtrage par entreprise)
/// </summary>
public interface IHasEntreprise
{
    string CodeEntreprise { get; set; }
}
