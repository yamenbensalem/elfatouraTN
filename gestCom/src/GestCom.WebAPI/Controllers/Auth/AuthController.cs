using GestCom.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GestCom.WebAPI.Controllers.Auth;

/// <summary>
/// Contrôleur pour l'authentification et la gestion des utilisateurs
/// </summary>
[Route("api/v1/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Authentifie un utilisateur et retourne un token JWT
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);

        if (!result.Succeeded)
        {
            return Unauthorized(new { error = result.Error });
        }

        var response = new LoginResponseDto
        {
            Token = result.Token!,
            RefreshToken = result.RefreshToken!,
            ExpiresAt = result.ExpiresAt!.Value,
            User = new UserInfoDto
            {
                Id = result.User!.Id,
                Email = result.User.Email,
                NomComplet = result.User.NomComplet,
                Role = result.User.Role,
                CodeEntreprise = result.User.CodeEntreprise
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Enregistre un nouvel utilisateur
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        if (request.Password != request.ConfirmPassword)
            return BadRequest(new { error = "Les mots de passe ne correspondent pas." });

        var registerRequest = new RegisterRequest
        {
            Email = request.Email,
            Password = request.Password,
            Nom = request.Nom,
            Prenom = request.Prenom,
            CodeEntreprise = request.CodeEntreprise,
            Role = "User"
        };

        var result = await _authService.RegisterAsync(registerRequest);

        if (!result.Succeeded)
        {
            if (result.Errors.Any())
                return BadRequest(new { errors = result.Errors });
            return Conflict(new { error = result.Error });
        }

        var response = new RegisterResponseDto
        {
            Id = result.User!.Id,
            Email = result.User.Email,
            NomComplet = result.User.NomComplet,
            Token = result.Token!,
            RefreshToken = result.RefreshToken!,
            Message = "Compte créé avec succès."
        };

        return CreatedAtAction(nameof(GetCurrentUser), response);
    }

    /// <summary>
    /// Rafraîchit le token JWT
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RefreshTokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (!result.Succeeded)
        {
            return Unauthorized(new { error = result.Error });
        }

        var response = new RefreshTokenResponseDto
        {
            Token = result.Token!,
            RefreshToken = result.RefreshToken!,
            ExpiresAt = result.ExpiresAt!.Value
        };

        return Ok(response);
    }

    /// <summary>
    /// Récupère les informations de l'utilisateur connecté
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserInfoDto>> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _authService.GetUserByIdAsync(userId);
        
        if (user == null)
            return NotFound();

        var response = new UserInfoDto
        {
            Id = user.Id,
            Email = user.Email,
            NomComplet = user.NomComplet,
            Role = user.Role,
            CodeEntreprise = user.CodeEntreprise
        };

        return Ok(response);
    }

    /// <summary>
    /// Déconnexion (révocation du token)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Logout([FromBody] LogoutRequestDto? request)
    {
        if (!string.IsNullOrEmpty(request?.RefreshToken))
        {
            await _authService.RevokeTokenAsync(request.RefreshToken);
        }
        
        return Ok(new { message = "Déconnexion réussie" });
    }

    /// <summary>
    /// Change le mot de passe de l'utilisateur connecté
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        if (request.NewPassword != request.ConfirmNewPassword)
            return BadRequest(new { error = "Les nouveaux mots de passe ne correspondent pas." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var success = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
        
        if (!success)
            return BadRequest(new { error = "Mot de passe actuel incorrect." });

        return Ok(new { message = "Mot de passe modifié avec succès" });
    }

    /// <summary>
    /// Demande de réinitialisation du mot de passe
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        var token = await _authService.GeneratePasswordResetTokenAsync(request.Email);
        
        // En production, envoyer l'email avec le token
        // Pour le développement, on log le token
        if (token != null)
        {
            _logger.LogInformation("Reset token for {Email}: {Token}", request.Email, token);
        }

        // Toujours retourner OK pour ne pas révéler si l'email existe
        return Ok(new { message = "Si l'email existe, un lien de réinitialisation a été envoyé." });
    }

    /// <summary>
    /// Réinitialise le mot de passe avec un token
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        if (request.NewPassword != request.ConfirmNewPassword)
            return BadRequest(new { error = "Les mots de passe ne correspondent pas." });

        var success = await _authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
        
        if (!success)
            return BadRequest(new { error = "Token invalide ou expiré." });

        return Ok(new { message = "Mot de passe réinitialisé avec succès" });
    }

    /// <summary>
    /// Liste tous les utilisateurs (Admin uniquement)
    /// </summary>
    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<UserInfoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserInfoDto>>> GetAllUsers([FromQuery] string? codeEntreprise)
    {
        var users = await _authService.GetAllUsersAsync(codeEntreprise);
        
        var response = users.Select(u => new UserInfoDto
        {
            Id = u.Id,
            Email = u.Email,
            NomComplet = u.NomComplet,
            Role = u.Role,
            CodeEntreprise = u.CodeEntreprise,
            Actif = u.Actif,
            DateCreation = u.DateCreation,
            DerniereConnexion = u.DerniereConnexion
        });

        return Ok(response);
    }

    /// <summary>
    /// Crée un nouvel utilisateur (Admin uniquement)
    /// </summary>
    [HttpPost("users")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserInfoDto>> CreateUser([FromBody] CreateUserRequestDto request)
    {
        var registerRequest = new RegisterRequest
        {
            Email = request.Email,
            Password = request.Password,
            Nom = request.Nom,
            Prenom = request.Prenom,
            CodeEntreprise = request.CodeEntreprise,
            Role = request.Role
        };

        var result = await _authService.RegisterAsync(registerRequest);

        if (!result.Succeeded)
        {
            if (result.Errors.Any())
                return BadRequest(new { errors = result.Errors });
            return BadRequest(new { error = result.Error });
        }

        var response = new UserInfoDto
        {
            Id = result.User!.Id,
            Email = result.User.Email,
            NomComplet = result.User.NomComplet,
            Role = result.User.Role,
            CodeEntreprise = result.User.CodeEntreprise
        };

        return CreatedAtAction(nameof(GetUserById), new { id = result.User.Id }, response);
    }

    /// <summary>
    /// Récupère un utilisateur par son ID (Admin uniquement)
    /// </summary>
    [HttpGet("users/{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserInfoDto>> GetUserById(string id)
    {
        var user = await _authService.GetUserByIdAsync(id);
        
        if (user == null)
            return NotFound();

        var response = new UserInfoDto
        {
            Id = user.Id,
            Email = user.Email,
            NomComplet = user.NomComplet,
            Role = user.Role,
            CodeEntreprise = user.CodeEntreprise,
            Actif = user.Actif,
            DateCreation = user.DateCreation,
            DerniereConnexion = user.DerniereConnexion
        };

        return Ok(response);
    }

    /// <summary>
    /// Met à jour le rôle d'un utilisateur (Admin uniquement)
    /// </summary>
    [HttpPut("users/{id}/role")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateUserRole(string id, [FromBody] UpdateUserRoleDto request)
    {
        var validRoles = new[] { "Admin", "Manager", "User" };
        if (!validRoles.Contains(request.Role))
            return BadRequest(new { error = $"Rôle invalide. Rôles valides: {string.Join(", ", validRoles)}" });

        var success = await _authService.UpdateUserRoleAsync(id, request.Role);
        
        if (!success)
            return NotFound(new { error = "Utilisateur non trouvé." });

        return Ok(new { message = $"Rôle mis à jour en {request.Role}" });
    }

    /// <summary>
    /// Désactive un utilisateur (Admin uniquement)
    /// </summary>
    [HttpDelete("users/{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeactivateUser(string id)
    {
        // Empêcher l'auto-désactivation
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (id == currentUserId)
            return BadRequest(new { error = "Vous ne pouvez pas désactiver votre propre compte." });

        var success = await _authService.DeactivateUserAsync(id);
        
        if (!success)
            return NotFound(new { error = "Utilisateur non trouvé." });

        return Ok(new { message = "Utilisateur désactivé" });
    }
}

#region DTOs

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserInfoDto User { get; set; } = new();
}

public class RegisterRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string? CodeEntreprise { get; set; }
}

public class RegisterResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NomComplet { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class CreateUserRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string? CodeEntreprise { get; set; }
    public string Role { get; set; } = "User";
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class UserInfoDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NomComplet { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public bool Actif { get; set; } = true;
    public DateTime DateCreation { get; set; }
    public DateTime? DerniereConnexion { get; set; }
}

public class LogoutRequestDto
{
    public string? RefreshToken { get; set; }
}

public class ChangePasswordRequestDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class ForgotPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequestDto
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class UpdateUserRoleDto
{
    public string Role { get; set; } = string.Empty;
}

#endregion
