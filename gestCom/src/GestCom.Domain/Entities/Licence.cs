using GestCom.Domain.Common;

namespace GestCom.Domain.Entities;

/// <summary>
/// Entité Licence - Gestion des licences
/// </summary>
public class Licence : BaseEntity
{
    public int Id { get; set; }
    public string CodeEntreprise { get; set; } = string.Empty;
    public string CleLicence { get; set; } = string.Empty;
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public string TypeLicence { get; set; } = string.Empty; // Trial, Standard, Premium
    public int NombreUtilisateurs { get; set; }
    public bool Actif { get; set; }
}
