using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;

namespace Web_T4C_GestCom.Services;

public interface IUtilisateurService
{
    Task<List<Utilisateur>> GetAllAsync();
    Task<Utilisateur?> GetByIdAsync(int id);
    Task<Utilisateur?> GetByLoginAsync(string login);
    Task<Utilisateur?> AuthentifierAsync(string login, string password);
    Task<bool> LoginExistsAsync(string login, int? excludeId = null);
    Task AddAsync(Utilisateur utilisateur, string plainPassword);
    Task UpdateAsync(Utilisateur utilisateur);
    Task ChangePasswordAsync(int id, string newPlainPassword);
    Task ActiverAsync(int id);
    Task DesactiverAsync(int id);
    Task SynchronizeLegacyRolesAsync();
    string HashPassword(string password);
}

public class UtilisateurService(AppDbContext db, IPermissionService? permissionService = null) : IUtilisateurService
{
    private const string HashPrefixBcryptV1 = "v2$bcrypt$";

    public async Task<List<Utilisateur>> GetAllAsync()
        => await db.Utilisateurs.OrderBy(u => u.Nom).ThenBy(u => u.Prenom).ToListAsync();

    public async Task<Utilisateur?> GetByIdAsync(int id)
        => await db.Utilisateurs.FindAsync(id);

    public async Task<Utilisateur?> GetByLoginAsync(string login)
        => await db.Utilisateurs.FirstOrDefaultAsync(u => u.Login == login);

    public async Task<Utilisateur?> AuthentifierAsync(string login, string password)
    {
        var user = await db.Utilisateurs.FirstOrDefaultAsync(u => u.Login == login && u.Actif);
        if (user is null)
            return null;

        if (!VerifyPassword(password, user.PasswordHash, out var needsRehash))
            return null;

        if (needsRehash)
        {
            user.PasswordHash = HashPassword(password);
            await db.SaveChangesAsync();
        }

        return user;
    }

    public async Task<bool> LoginExistsAsync(string login, int? excludeId = null)
    {
        var query = db.Utilisateurs.Where(u => u.Login == login);
        if (excludeId.HasValue)
            query = query.Where(u => u.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task AddAsync(Utilisateur utilisateur, string plainPassword)
    {
        utilisateur.Role = RoleNameMapper.NormalizeKnownRoleName(utilisateur.Role);
        utilisateur.PasswordHash = HashPassword(plainPassword);
        utilisateur.DateCreation = DateTime.Now;
        db.Utilisateurs.Add(utilisateur);
        await db.SaveChangesAsync();

        await SyncUserRoleMappingAsync(utilisateur.Id, utilisateur.Role);
    }

    public async Task UpdateAsync(Utilisateur utilisateur)
    {
        utilisateur.Role = RoleNameMapper.NormalizeKnownRoleName(utilisateur.Role);
        db.Utilisateurs.Update(utilisateur);
        await db.SaveChangesAsync();

        await SyncUserRoleMappingAsync(utilisateur.Id, utilisateur.Role);
    }

    public async Task ChangePasswordAsync(int id, string newPlainPassword)
    {
        var u = await db.Utilisateurs.FindAsync(id)
            ?? throw new InvalidOperationException("Utilisateur introuvable.");
        u.PasswordHash = HashPassword(newPlainPassword);
        await db.SaveChangesAsync();
    }

    public async Task ActiverAsync(int id)
    {
        var u = await db.Utilisateurs.FindAsync(id)
            ?? throw new InvalidOperationException("Utilisateur introuvable.");
        u.Actif = true;
        await db.SaveChangesAsync();
    }

    public async Task DesactiverAsync(int id)
    {
        var u = await db.Utilisateurs.FindAsync(id)
            ?? throw new InvalidOperationException("Utilisateur introuvable.");
        u.Actif = false;
        await db.SaveChangesAsync();
    }

    public async Task SynchronizeLegacyRolesAsync()
    {
        var users = await db.Utilisateurs
            .Select(u => new { u.Id, u.Role })
            .ToListAsync();

        if (users.Count == 0)
            return;

        foreach (var user in users)
            await SyncUserRoleMappingAsync(user.Id, user.Role, saveImmediately: false);

        await db.SaveChangesAsync();
    }

    public string HashPassword(string password)
    {
        var bcryptHash = BCrypt.Net.BCrypt.HashPassword(password);
        return HashPrefixBcryptV1 + bcryptHash;
    }

    private static bool VerifyPassword(string plainPassword, string storedHash, out bool needsRehash)
    {
        needsRehash = false;
        if (string.IsNullOrWhiteSpace(storedHash))
            return false;

        if (storedHash.StartsWith(HashPrefixBcryptV1, StringComparison.Ordinal))
        {
            var payload = storedHash[HashPrefixBcryptV1.Length..];
            return BCrypt.Net.BCrypt.Verify(plainPassword, payload);
        }

        // Compatibility: accept bare bcrypt payload hashes.
        if (storedHash.StartsWith("$2", StringComparison.Ordinal))
        {
            needsRehash = true;
            return BCrypt.Net.BCrypt.Verify(plainPassword, storedHash);
        }

        // Legacy v1 format: unsalted SHA-256 hex digest.
        var sha256Hex = ComputeLegacySha256Hex(plainPassword);
        if (!string.Equals(sha256Hex, storedHash, StringComparison.OrdinalIgnoreCase))
            return false;

        needsRehash = true;
        return true;
    }

    private static string ComputeLegacySha256Hex(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private async Task SyncUserRoleMappingAsync(int userId, string? legacyRoleName, bool saveImmediately = true)
    {
        var roleName = RoleNameMapper.NormalizeKnownRoleName(legacyRoleName);
        var roleId = await ResolveRoleIdAsync(roleName)
            ?? await ResolveRoleIdAsync(RoleNameMapper.Employe)
            ?? throw new InvalidOperationException("Aucun rôle RBAC valide n'est configuré dans app_role.");

        var existingMappings = await db.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToListAsync();

        var hasTarget = existingMappings.Any(ur => ur.RoleId == roleId);
        if (!hasTarget)
            db.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });

        var staleMappings = existingMappings.Where(ur => ur.RoleId != roleId).ToList();
        if (staleMappings.Count > 0)
            db.UserRoles.RemoveRange(staleMappings);

        if (saveImmediately)
            await db.SaveChangesAsync();

        permissionService?.InvalidateUser(userId);
    }

    private async Task<int?> ResolveRoleIdAsync(string roleName)
    {
        var id = await db.AppRoles
            .Where(r => r.Name == roleName)
            .Select(r => (int?)r.Id)
            .FirstOrDefaultAsync();

        return id;
    }
}
