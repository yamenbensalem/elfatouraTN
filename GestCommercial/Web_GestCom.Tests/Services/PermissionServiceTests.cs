using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;
using Web_GestCom.Tests.Helpers;
using Xunit;

namespace Web_GestCom.Tests.Services;

/// <summary>
/// Tests unitaires pour PermissionService.
/// Utilise EF Core In-Memory (via DbContextFactory.Create) + IMemoryCache réel.
/// </summary>
public class PermissionServiceTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static (PermissionService svc, AppDbContext db) CreateSvc(string? dbName = null)
    {
        var name    = dbName ?? Guid.NewGuid().ToString();
        var db      = DbContextFactory.Create(name); // seed + keep open for test assertions
        var cache   = new MemoryCache(new MemoryCacheOptions());
        var factory = new InMemoryDbContextFactory(name);
        var svc     = new PermissionService(factory, cache);
        return (svc, db);
    }

    /// <summary>
    /// Insère un utilisateur, un rôle et une permission, et relie le tout.
    /// Retourne l'id utilisateur.
    /// </summary>
    private static int SeedUserWithPermission(AppDbContext db,
                                               string feature,
                                               string action,
                                               string roleName = "TestRole")
    {
        // Permission
        var permId = db.Permissions.Count() + 100;
        var perm = new Permission { Id = permId, Feature = feature, Action = action };
        db.Permissions.Add(perm);

        // Role
        var roleId = db.AppRoles.Count() + 100;
        var role = new AppRole { Id = roleId, Name = roleName, CompanyId = null };
        db.AppRoles.Add(role);

        // RolePermission
        db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permId });

        // User
        var user = new Utilisateur
        {
            Login  = $"user_{Guid.NewGuid():N}",
            Prenom = "Test",
            Nom    = "User",
            Role   = roleName,
            Actif  = true
        };
        db.Utilisateurs.Add(user);
        db.SaveChanges();

        // UserRole
        db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId });
        db.SaveChanges();

        return user.Id;
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task HasPermission_ReturnsTrue_WhenUserHasPermission()
    {
        var (svc, db) = CreateSvc();
        var userId = SeedUserWithPermission(db, "factures", "view");

        var result = await svc.HasPermissionAsync(userId, "factures.view");

        Assert.True(result);
    }

    [Fact]
    public async Task HasPermission_ReturnsFalse_WhenUserLacksPermission()
    {
        var (svc, db) = CreateSvc();
        var userId = SeedUserWithPermission(db, "factures", "view");

        var result = await svc.HasPermissionAsync(userId, "factures.delete");

        Assert.False(result);
    }

    [Fact]
    public async Task GetUserPermissions_ReturnsAllGrantedPermissions()
    {
        var (svc, db) = CreateSvc();
        var userId = SeedUserWithPermission(db, "devis", "create");

        var perms = (await svc.GetUserPermissionsAsync(userId)).ToList();

        Assert.Contains("devis.create", perms);
    }

    [Fact]
    public async Task GetUserPermissions_IsCached_SecondCallReturnsSameResult()
    {
        var (svc, db) = CreateSvc();
        var userId = SeedUserWithPermission(db, "commandes-vente", "view");

        var first  = (await svc.GetUserPermissionsAsync(userId)).ToList();

        // Add a permission directly — cache should hide it on second call
        var newPerm = new Permission { Id = 999, Feature = "commandes-vente", Action = "delete" };
        db.Permissions.Add(newPerm);
        var roleId = db.UserRoles.First(ur => ur.UserId == userId).RoleId;
        db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = 999 });
        db.SaveChanges();

        var second = (await svc.GetUserPermissionsAsync(userId)).ToList();

        Assert.Equal(first.Count, second.Count); // still from cache
    }

    [Fact]
    public async Task InvalidateUser_ClearsCache_NextCallReadsDb()
    {
        var (svc, db) = CreateSvc();
        var userId = SeedUserWithPermission(db, "bons-livraison", "view");

        var first = (await svc.GetUserPermissionsAsync(userId)).ToList();

        // Add a permission
        var newPerm = new Permission { Id = 998, Feature = "bons-livraison", Action = "create" };
        db.Permissions.Add(newPerm);
        var roleId = db.UserRoles.First(ur => ur.UserId == userId).RoleId;
        db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = 998 });
        db.SaveChanges();

        svc.InvalidateUser(userId); // clear cache

        var second = (await svc.GetUserPermissionsAsync(userId)).ToList();

        Assert.True(second.Count > first.Count);
    }

    [Fact]
    public async Task HasPermission_ReturnsFalse_ForNonExistentUser()
    {
        var (svc, _) = CreateSvc();

        var result = await svc.HasPermissionAsync(99999, "factures.view");

        Assert.False(result);
    }
}
