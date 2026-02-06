using GestCom.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace GestCom.Infrastructure.Identity;

/// <summary>
/// Utilisateur de l'application (extends ASP.NET Identity)
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public bool Actif { get; set; } = true;
    public DateTime DateCreation { get; set; } = DateTime.Now;
    public DateTime? DerniereConnexion { get; set; }
    
    // Navigation properties
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
