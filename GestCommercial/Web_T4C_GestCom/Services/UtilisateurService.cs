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
    string HashPassword(string password);
}

public class UtilisateurService(AppDbContext db) : IUtilisateurService
{
    public async Task<List<Utilisateur>> GetAllAsync()
        => await db.Utilisateurs.OrderBy(u => u.Nom).ThenBy(u => u.Prenom).ToListAsync();

    public async Task<Utilisateur?> GetByIdAsync(int id)
        => await db.Utilisateurs.FindAsync(id);

    public async Task<Utilisateur?> GetByLoginAsync(string login)
        => await db.Utilisateurs.FirstOrDefaultAsync(u => u.Login == login);

    public async Task<Utilisateur?> AuthentifierAsync(string login, string password)
    {
        var hash = HashPassword(password);
        return await db.Utilisateurs.FirstOrDefaultAsync(
            u => u.Login == login && u.PasswordHash == hash && u.Actif);
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
        utilisateur.PasswordHash = HashPassword(plainPassword);
        utilisateur.DateCreation = DateTime.Now;
        db.Utilisateurs.Add(utilisateur);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Utilisateur utilisateur)
    {
        db.Utilisateurs.Update(utilisateur);
        await db.SaveChangesAsync();
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

    public string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }
}
