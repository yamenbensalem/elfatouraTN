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
    Task<string> GetPrimaryRoleNameAsync(int userId, bool isSuperAdmin);
    [Obsolete("UserRole/AppRole is now the authoritative source. This method will be removed once all deployments have been migrated.")]
    Task SynchronizeLegacyRolesAsync();
    string HashPassword(string password);
}

public class UtilisateurService(
    AppDbContext db,
    ITenantService? tenantService = null,
    IPermissionService? permissionService = null) : IUtilisateurService
{
    private const string HashPrefixBcryptV1 = "v2$bcrypt$";

    public async Task<List<Utilisateur>> GetAllAsync()
    {
        var query = db.Utilisateurs.AsNoTracking().AsQueryable();
        if (tenantService?.CurrentCompanyId is int companyId)
            query = query.Where(u => !u.IsSuperAdmin && u.CompanyId == companyId);

        return await query.OrderBy(u => u.Nom).ThenBy(u => u.Prenom).ToListAsync();
    }

    public async Task<Utilisateur?> GetByIdAsync(int id)
    {
        var query = db.Utilisateurs.AsNoTracking().Where(u => u.Id == id);
        if (tenantService?.CurrentCompanyId is int companyId)
            query = query.Where(u => !u.IsSuperAdmin && u.CompanyId == companyId);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<Utilisateur?> GetByLoginAsync(string login)
        => await db.Utilisateurs.AsNoTracking().FirstOrDefaultAsync(u => u.Login == login);

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
            await db.SaveChangesGuardedAsync();
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
        await EnsureTenantDefaultsAsync(utilisateur);

        utilisateur.Role = utilisateur.IsSuperAdmin
            ? RoleNameMapper.SuperAdmin
            : RoleNameMapper.NormalizeKnownRoleName(utilisateur.Role);

        utilisateur.PasswordHash = HashPassword(plainPassword);
        utilisateur.DateCreation = DateTime.Now;
        utilisateur.SecurityStamp = CreateSecurityStamp();
        utilisateur.PermissionsVersion = Math.Max(utilisateur.PermissionsVersion, 1);

        db.Utilisateurs.Add(utilisateur);
        await db.SaveChangesGuardedAsync();

        await SyncUserRoleMappingAsync(utilisateur.Id, utilisateur.Role);
    }

    public async Task UpdateAsync(Utilisateur utilisateur)
    {
        var previous = await db.Utilisateurs
            .AsNoTracking()
            .Where(u => u.Id == utilisateur.Id)
            .Select(u => new
            {
                u.Role,
                u.CompanyId,
                u.IsSuperAdmin,
                u.PermissionsVersion
            })
            .FirstOrDefaultAsync();

        await EnsureTenantDefaultsAsync(utilisateur);

        utilisateur.Role = utilisateur.IsSuperAdmin
            ? RoleNameMapper.SuperAdmin
            : RoleNameMapper.NormalizeKnownRoleName(utilisateur.Role);

        utilisateur.SecurityStamp = string.IsNullOrWhiteSpace(utilisateur.SecurityStamp)
            ? CreateSecurityStamp()
            : utilisateur.SecurityStamp;

        utilisateur.PermissionsVersion = Math.Max(utilisateur.PermissionsVersion, 1);

        var authStateChanged = previous is not null &&
                               (previous.Role != utilisateur.Role ||
                                previous.CompanyId != utilisateur.CompanyId ||
                                previous.IsSuperAdmin != utilisateur.IsSuperAdmin);

        if (authStateChanged)
        {
            utilisateur.PermissionsVersion = Math.Max(utilisateur.PermissionsVersion, previous!.PermissionsVersion + 1);
            utilisateur.SecurityStamp = CreateSecurityStamp();
        }

        db.Utilisateurs.Update(utilisateur);
        await db.SaveChangesGuardedAsync();

        await SyncUserRoleMappingAsync(utilisateur.Id, utilisateur.Role);
    }

    public async Task ChangePasswordAsync(int id, string newPlainPassword)
    {
        var u = await db.Utilisateurs.FindAsync(id)
            ?? throw new InvalidOperationException("Utilisateur introuvable.");
        u.PasswordHash = HashPassword(newPlainPassword);
        u.SecurityStamp = CreateSecurityStamp();
        await db.SaveChangesGuardedAsync();
    }

    public async Task ActiverAsync(int id)
    {
        var u = await db.Utilisateurs.FindAsync(id)
            ?? throw new InvalidOperationException("Utilisateur introuvable.");
        u.Actif = true;
        u.SecurityStamp = CreateSecurityStamp();
        await db.SaveChangesGuardedAsync();
    }

    public async Task DesactiverAsync(int id)
    {
        var u = await db.Utilisateurs.FindAsync(id)
            ?? throw new InvalidOperationException("Utilisateur introuvable.");
        u.Actif = false;
        u.SecurityStamp = CreateSecurityStamp();
        await db.SaveChangesGuardedAsync();
    }

    public async Task<string> GetPrimaryRoleNameAsync(int userId, bool isSuperAdmin)
    {
        if (isSuperAdmin)
            return RoleNameMapper.SuperAdmin;

        var roleName = await db.UserRoles
            .Where(ur => ur.UserId == userId && ur.Role != null)
            .OrderBy(ur => ur.Role!.CompanyId == null ? 0 : 1)
            .Select(ur => ur.Role!.Name)
            .FirstOrDefaultAsync();

        return RoleNameMapper.NormalizeKnownRoleName(roleName);
    }

    [Obsolete("UserRole/AppRole is now the authoritative source. This method will be removed once all deployments have been migrated.")]
    public async Task SynchronizeLegacyRolesAsync()
    {
        var users = await db.Utilisateurs
            .Select(u => new { u.Id, u.Role })
            .ToListAsync();

        if (users.Count == 0)
            return;

        foreach (var user in users)
            await SyncUserRoleMappingAsync(user.Id, user.Role, saveImmediately: false);

        await db.SaveChangesGuardedAsync();
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
        var userMeta = await db.Utilisateurs
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.CompanyId, u.IsSuperAdmin })
            .FirstOrDefaultAsync();

        var companyId = userMeta?.CompanyId;
        var isSuperAdmin = userMeta?.IsSuperAdmin ?? false;

        var roleName = isSuperAdmin
            ? RoleNameMapper.SuperAdmin
            : RoleNameMapper.NormalizeKnownRoleName(legacyRoleName);

        var roleId = await ResolveRoleIdAsync(roleName, companyId)
            ?? await ResolveRoleIdAsync(RoleNameMapper.Employe, companyId)
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
            await db.SaveChangesGuardedAsync();

        permissionService?.InvalidateUser(userId);
    }

    private async Task<int?> ResolveRoleIdAsync(string roleName, int? companyId)
    {
        var id = await db.AppRoles
            .Where(r => r.Name == roleName)
            .Where(r => r.CompanyId == null || (companyId.HasValue && r.CompanyId == companyId))
            .OrderBy(r => r.CompanyId == companyId ? 0 : 1)
            .Select(r => (int?)r.Id)
            .FirstOrDefaultAsync();

        return id;
    }

    private async Task EnsureTenantDefaultsAsync(Utilisateur utilisateur)
    {
        if (utilisateur.IsSuperAdmin)
        {
            utilisateur.CompanyId = null;
            return;
        }

        if (utilisateur.CompanyId.HasValue)
            return;

        if (tenantService?.CurrentCompanyId is int currentCompanyId)
        {
            utilisateur.CompanyId = currentCompanyId;
            return;
        }

        utilisateur.CompanyId = await db.Companies
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync();
    }

    private static string CreateSecurityStamp() => Guid.NewGuid().ToString("N");
}
