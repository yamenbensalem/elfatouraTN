using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;
using Web_T4C_GestCom.Tests.Helpers;
using Xunit;

namespace Web_T4C_GestCom.Tests.Services;

public class UtilisateurServiceTests
{
    private static UtilisateurService CreateService(out AppDbContext db)
    {
        db = DbContextFactory.Create();
        return new UtilisateurService(db);
    }

    private static Utilisateur MakeUser(string login, string role = "Employé")
        => new()
        {
            Login = login,
            Prenom = "Test",
            Nom = "User",
            Role = role,
            Actif = true,
            DateCreation = DateTime.UtcNow
        };

    [Fact]
    public async Task AddAsync_StoresVersionedBcryptHash_AndAuthenticates()
    {
        var svc = CreateService(out var db);
        var user = MakeUser("bcrypt-user");

        await svc.AddAsync(user, "P@ssw0rd!");

        var stored = await db.Utilisateurs.SingleAsync(u => u.Login == "bcrypt-user");
        Assert.StartsWith("v2$bcrypt$", stored.PasswordHash, StringComparison.Ordinal);
        Assert.NotEqual(LegacySha256Hex("P@ssw0rd!"), stored.PasswordHash);

        var auth = await svc.AuthentifierAsync("bcrypt-user", "P@ssw0rd!");
        Assert.NotNull(auth);
    }

    [Fact]
    public async Task AuthentifierAsync_LegacySha256_RehashesToBcryptAfterSuccessfulLogin()
    {
        var svc = CreateService(out var db);
        var legacyHash = LegacySha256Hex("Legacy#123");

        db.Utilisateurs.Add(new Utilisateur
        {
            Login = "legacy-user",
            PasswordHash = legacyHash,
            Prenom = "Legacy",
            Nom = "User",
            Role = "Employé",
            Actif = true,
            DateCreation = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var auth = await svc.AuthentifierAsync("legacy-user", "Legacy#123");

        Assert.NotNull(auth);
        var updated = await db.Utilisateurs.SingleAsync(u => u.Login == "legacy-user");
        Assert.StartsWith("v2$bcrypt$", updated.PasswordHash, StringComparison.Ordinal);
        Assert.NotEqual(legacyHash, updated.PasswordHash);

        // Ensure subsequent login works with migrated hash.
        var secondAuth = await svc.AuthentifierAsync("legacy-user", "Legacy#123");
        Assert.NotNull(secondAuth);
    }

    [Fact]
    public async Task AuthentifierAsync_LegacySha256_WrongPassword_DoesNotRehash()
    {
        var svc = CreateService(out var db);
        var legacyHash = LegacySha256Hex("RightPassword");

        db.Utilisateurs.Add(new Utilisateur
        {
            Login = "legacy-wrong",
            PasswordHash = legacyHash,
            Prenom = "Legacy",
            Nom = "Wrong",
            Role = "Employé",
            Actif = true,
            DateCreation = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var auth = await svc.AuthentifierAsync("legacy-wrong", "WrongPassword");

        Assert.Null(auth);
        var unchanged = await db.Utilisateurs.SingleAsync(u => u.Login == "legacy-wrong");
        Assert.Equal(legacyHash, unchanged.PasswordHash);
    }

    private static string LegacySha256Hex(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
