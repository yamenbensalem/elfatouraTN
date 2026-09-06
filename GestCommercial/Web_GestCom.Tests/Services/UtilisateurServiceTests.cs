using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;
using Web_GestCom.Tests.Helpers;
using Xunit;

namespace Web_GestCom.Tests.Services;

public class UtilisateurServiceTests
{
    private static UtilisateurService CreateService(out AppDbContext db, ITenantService? tenantService = null)
    {
        db = DbContextFactory.Create();
        return new UtilisateurService(db, tenantService ?? new StubTenantService());
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
    public async Task AddAsync_WithoutExplicitCompany_InheritsActingAdminsTenant()
    {
        var svc = CreateService(out var db, new StubTenantService(companyId: 7));
        var user = MakeUser("no-company-user");

        await svc.AddAsync(user, "P@ssw0rd!");

        var stored = await db.Utilisateurs.SingleAsync(u => u.Login == "no-company-user");
        Assert.Equal(7, stored.CompanyId);
    }

    [Fact]
    public async Task AddAsync_WithExplicitCompany_PreservesIt_EvenWhenActingTenantDiffers()
    {
        // SuperAdmin path: the form lets a SuperAdmin pick any company explicitly, and that must
        // win over whatever the acting session's own (nonexistent) tenant would otherwise supply.
        var svc = CreateService(out var db, new StubTenantService(companyId: 7));
        var user = MakeUser("explicit-company-user");
        user.CompanyId = 42;

        await svc.AddAsync(user, "P@ssw0rd!");

        var stored = await db.Utilisateurs.SingleAsync(u => u.Login == "explicit-company-user");
        Assert.Equal(42, stored.CompanyId);
    }

    [Fact]
    public async Task AddAsync_SuperAdmin_ForcesCompanyIdNull_EvenIfOneWasSet()
    {
        var svc = CreateService(out var db, new StubTenantService(companyId: 7));
        var user = MakeUser("superadmin-user");
        user.IsSuperAdmin = true;
        user.CompanyId = 42;

        await svc.AddAsync(user, "P@ssw0rd!");

        var stored = await db.Utilisateurs.SingleAsync(u => u.Login == "superadmin-user");
        Assert.Null(stored.CompanyId);
    }

    [Fact]
    public async Task AddAsync_NoExplicitCompanyAndNoActiveTenant_ThrowsInsteadOfGuessing()
    {
        // Bypasses CreateService's StubTenantService default on purpose — this exercises the
        // actual "no tenant service at all" case (matches `new UtilisateurService(db)` with the
        // constructor's own default), which must fail loudly rather than pick an arbitrary company.
        var db = DbContextFactory.Create();
        var svc = new UtilisateurService(db);
        var user = MakeUser("orphan-user");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.AddAsync(user, "P@ssw0rd!"));
        Assert.Contains("entreprise", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetAllAsync_IncludesCompanyNavigation()
    {
        var svc = CreateService(out var db, new StubTenantService(companyId: null));
        db.Companies.Add(new Company { Id = 99, Name = "Société Test" });
        await db.SaveChangesAsync();
        db.Utilisateurs.Add(new Utilisateur
        {
            Login = "with-company", Prenom = "A", Nom = "B", Role = "Employé",
            Actif = true, DateCreation = DateTime.UtcNow, CompanyId = 99
        });
        await db.SaveChangesAsync();

        var all = await svc.GetAllAsync();

        var found = Assert.Single(all, u => u.Login == "with-company");
        Assert.Equal("Société Test", found.Company?.Name);
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
