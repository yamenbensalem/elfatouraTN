using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GestCom.Application.Common.Interfaces;
using GestCom.Infrastructure.Data;
using GestCom.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace GestCom.Infrastructure.Services;

/// <summary>
/// Service d'authentification avec JWT
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            
            if (user == null)
            {
                _logger.LogWarning("Tentative de connexion avec email inexistant: {Email}", email);
                return new AuthResult { Succeeded = false, Error = "Email ou mot de passe incorrect." };
            }

            if (!user.Actif)
            {
                _logger.LogWarning("Tentative de connexion avec compte désactivé: {Email}", email);
                return new AuthResult { Succeeded = false, Error = "Ce compte a été désactivé." };
            }

            var result = await _userManager.CheckPasswordAsync(user, password);
            
            if (!result)
            {
                _logger.LogWarning("Mot de passe incorrect pour: {Email}", email);
                return new AuthResult { Succeeded = false, Error = "Email ou mot de passe incorrect." };
            }

            // Mettre à jour la dernière connexion
            user.DerniereConnexion = DateTime.Now;
            await _userManager.UpdateAsync(user);

            // Générer les tokens
            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);
            var refreshToken = await GenerateRefreshTokenAsync(user.Id);

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var expirationHours = int.Parse(jwtSettings["ExpirationInHours"] ?? "8");

            _logger.LogInformation("Connexion réussie pour: {Email}", email);

            return new AuthResult
            {
                Succeeded = true,
                Token = token,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddHours(expirationHours),
                User = MapToUserDto(user, roles.FirstOrDefault() ?? "User")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la connexion pour: {Email}", email);
            return new AuthResult { Succeeded = false, Error = "Une erreur est survenue lors de la connexion." };
        }
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // Vérifier si l'email existe déjà
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new AuthResult { Succeeded = false, Error = "Cet email est déjà utilisé." };
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                Nom = request.Nom,
                Prenom = request.Prenom,
                CodeEntreprise = request.CodeEntreprise ?? string.Empty,
                Actif = true,
                DateCreation = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                _logger.LogWarning("Échec de création d'utilisateur: {Errors}", string.Join(", ", errors));
                return new AuthResult { Succeeded = false, Errors = errors };
            }

            // Assigner le rôle
            var role = request.Role;
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
            await _userManager.AddToRoleAsync(user, role);

            // Générer les tokens
            var token = GenerateJwtToken(user, new[] { role });
            var refreshToken = await GenerateRefreshTokenAsync(user.Id);

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var expirationHours = int.Parse(jwtSettings["ExpirationInHours"] ?? "8");

            _logger.LogInformation("Nouvel utilisateur créé: {Email}", request.Email);

            return new AuthResult
            {
                Succeeded = true,
                Token = token,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddHours(expirationHours),
                User = MapToUserDto(user, role)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'inscription: {Email}", request.Email);
            return new AuthResult { Succeeded = false, Error = "Une erreur est survenue lors de l'inscription." };
        }
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var storedToken = await _context.Set<RefreshToken>()
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null)
            {
                return new AuthResult { Succeeded = false, Error = "Token de rafraîchissement invalide." };
            }

            if (!storedToken.IsActive)
            {
                return new AuthResult { Succeeded = false, Error = "Token de rafraîchissement expiré ou révoqué." };
            }

            var user = storedToken.User;
            if (!user.Actif)
            {
                return new AuthResult { Succeeded = false, Error = "Ce compte a été désactivé." };
            }

            // Révoquer l'ancien token
            storedToken.Revoked = DateTime.UtcNow;
            storedToken.ReasonRevoked = "Remplacé par un nouveau token";

            // Générer de nouveaux tokens
            var roles = await _userManager.GetRolesAsync(user);
            var newJwtToken = GenerateJwtToken(user, roles);
            var newRefreshToken = await GenerateRefreshTokenAsync(user.Id);

            storedToken.ReplacedByToken = newRefreshToken.Token;
            await _context.SaveChangesAsync();

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var expirationHours = int.Parse(jwtSettings["ExpirationInHours"] ?? "8");

            return new AuthResult
            {
                Succeeded = true,
                Token = newJwtToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddHours(expirationHours),
                User = MapToUserDto(user, roles.FirstOrDefault() ?? "User")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du rafraîchissement du token");
            return new AuthResult { Succeeded = false, Error = "Une erreur est survenue." };
        }
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        var storedToken = await _context.Set<RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken != null && storedToken.IsActive)
        {
            storedToken.Revoked = DateTime.UtcNow;
            storedToken.ReasonRevoked = "Révoqué par l'utilisateur";
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded;
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return false;

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded;
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return null;

        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return MapToUserDto(user, roles.FirstOrDefault() ?? "User");
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync(string? codeEntreprise = null)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrEmpty(codeEntreprise))
        {
            query = query.Where(u => u.CodeEntreprise == codeEntreprise);
        }

        var users = await query.ToListAsync();
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(MapToUserDto(user, roles.FirstOrDefault() ?? "User"));
        }

        return userDtos;
    }

    public async Task<bool> UpdateUserRoleAsync(string userId, string newRole)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        // Supprimer les rôles existants
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        // Ajouter le nouveau rôle
        if (!await _roleManager.RoleExistsAsync(newRole))
        {
            await _roleManager.CreateAsync(new IdentityRole(newRole));
        }

        var result = await _userManager.AddToRoleAsync(user, newRole);
        return result.Succeeded;
    }

    public async Task<bool> DeactivateUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        user.Actif = false;
        var result = await _userManager.UpdateAsync(user);

        // Révoquer tous les refresh tokens de l'utilisateur
        var tokens = await _context.Set<RefreshToken>()
            .Where(rt => rt.UserId == userId && rt.Revoked == null)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.Revoked = DateTime.UtcNow;
            token.ReasonRevoked = "Compte désactivé";
        }

        await _context.SaveChangesAsync();

        return result.Succeeded;
    }

    #region Private Methods

    private string GenerateJwtToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expirationHours = int.Parse(jwtSettings["ExpirationInHours"] ?? "8");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, $"{user.Prenom} {user.Nom}".Trim()),
            new("CodeEntreprise", user.CodeEntreprise),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Ajouter les rôles
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expirationHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(string userId)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var refreshTokenExpirationDays = int.Parse(jwtSettings["RefreshTokenExpirationInDays"] ?? "7");

        var refreshToken = new RefreshToken
        {
            Token = GenerateSecureRandomToken(),
            UserId = userId,
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(refreshTokenExpirationDays)
        };

        _context.Set<RefreshToken>().Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    private static string GenerateSecureRandomToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static UserDto MapToUserDto(ApplicationUser user, string role)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Nom = user.Nom,
            Prenom = user.Prenom,
            CodeEntreprise = user.CodeEntreprise,
            Role = role,
            Actif = user.Actif,
            DateCreation = user.DateCreation,
            DerniereConnexion = user.DerniereConnexion
        };
    }

    #endregion
}
