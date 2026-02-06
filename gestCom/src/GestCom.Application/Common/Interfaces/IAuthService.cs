namespace GestCom.Application.Common.Interfaces;

/// <summary>
/// Interface pour le service d'authentification
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authentifie un utilisateur
    /// </summary>
    Task<AuthResult> LoginAsync(string email, string password);

    /// <summary>
    /// Enregistre un nouvel utilisateur
    /// </summary>
    Task<AuthResult> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Rafraîchit le token JWT
    /// </summary>
    Task<AuthResult> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Révoque un refresh token
    /// </summary>
    Task RevokeTokenAsync(string refreshToken);

    /// <summary>
    /// Change le mot de passe
    /// </summary>
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

    /// <summary>
    /// Réinitialise le mot de passe
    /// </summary>
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);

    /// <summary>
    /// Génère un token de réinitialisation
    /// </summary>
    Task<string?> GeneratePasswordResetTokenAsync(string email);

    /// <summary>
    /// Récupère un utilisateur par son ID
    /// </summary>
    Task<UserDto?> GetUserByIdAsync(string userId);

    /// <summary>
    /// Récupère tous les utilisateurs
    /// </summary>
    Task<IEnumerable<UserDto>> GetAllUsersAsync(string? codeEntreprise = null);

    /// <summary>
    /// Met à jour le rôle d'un utilisateur
    /// </summary>
    Task<bool> UpdateUserRoleAsync(string userId, string newRole);

    /// <summary>
    /// Désactive un utilisateur
    /// </summary>
    Task<bool> DeactivateUserAsync(string userId);
}

#region DTOs & Models

public class AuthResult
{
    public bool Succeeded { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserDto? User { get; set; }
    public string? Error { get; set; }
    public IEnumerable<string> Errors { get; set; } = Array.Empty<string>();
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string? CodeEntreprise { get; set; }
    public string Role { get; set; } = "User";
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string NomComplet => $"{Prenom} {Nom}".Trim();
    public string CodeEntreprise { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool Actif { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime? DerniereConnexion { get; set; }
}

#endregion
