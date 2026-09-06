using Microsoft.EntityFrameworkCore;
using GestCom_Desktop.Session;
using GestCom_Desktop.Tests.Helpers;
using Web_GestCom.Data.Models;
using Xunit;

namespace GestCom_Desktop.Tests.Data;

/// <summary>
/// Proves that AppDbContext's tenant isolation (query filters + ApplyTenantOwnershipRules) works
/// correctly when driven by the desktop's UserSession/DesktopExecutionContext, exactly like it does
/// with the web's HttpExecutionContext (see Web_GestCom.Tests' equivalent suite). Also documents,
/// as a regression test, the exact bug found during the CRUD audit: ClientService.UpdateAsync does
/// db.Clients.Update(client) — a full-entity replace — so building a *fresh* Client for an update
/// (CompanyId left at its default, null) throws a cross-tenant exception, while loading the existing
/// entity and mutating it (what ClientEditForm now does) succeeds.
/// </summary>
public class AppDbContextTenantIsolationTests
{
    [Fact]
    public async Task SaveChangesAsync_WhenAddingTenantOwnedEntity_StampsCurrentCompanyId()
    {
        // Arrange
        var session = TenantSession(companyId: 1);
        using var context = DbContextFactory.Create(new DesktopExecutionContext(session));

        var client = new Client { CodeClient = "CL00001", NomClient = "Client Tenant 1", CodeDevise = 1 };

        // Act
        context.Clients.Add(client);
        await context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, client.CompanyId);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenUpdatingWithFreshEntityMissingCompanyId_ThrowsUnauthorizedAccessException()
    {
        // Arrange — this is the exact shape of the bug found in ClientEditForm.SaveAsync before the fix:
        // a brand-new Client built from form fields alone, CompanyId defaulting to null.
        var dbName = Guid.NewGuid().ToString();
        using (var seed = DbContextFactory.Create(dbName: dbName))
        {
            seed.Clients.Add(new Client { CodeClient = "CL00001", NomClient = "Original", CodeDevise = 1, CompanyId = 1 });
            await seed.SaveChangesAsync();
        }

        var session = TenantSession(companyId: 1);
        using var context = DbContextFactory.Create(new DesktopExecutionContext(session), dbName);
        var freshClient = new Client { CodeClient = "CL00001", NomClient = "Modifié sans CompanyId", CodeDevise = 1 };

        // Act
        context.Clients.Update(freshClient);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChangesAsync_WhenUpdatingLoadedEntity_PreservesCompanyIdAndSucceeds()
    {
        // Arrange — this is the fixed shape: load the existing entity, mutate its fields, update that.
        var dbName = Guid.NewGuid().ToString();
        using (var seed = DbContextFactory.Create(dbName: dbName))
        {
            seed.Clients.Add(new Client { CodeClient = "CL00001", NomClient = "Original", CodeDevise = 1, CompanyId = 1 });
            await seed.SaveChangesAsync();
        }

        var session = TenantSession(companyId: 1);
        using var context = DbContextFactory.Create(new DesktopExecutionContext(session), dbName);
        var loaded = await context.Clients.FirstAsync(c => c.CodeClient == "CL00001");
        loaded.NomClient = "Modifié correctement";

        // Act
        context.Clients.Update(loaded);
        await context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, loaded.CompanyId);
        Assert.Equal("Modifié correctement", loaded.NomClient);
    }

    [Fact]
    public async Task QueryFilter_WhenTenantUser_ReturnsOnlyCurrentTenantRows()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using (var seed = DbContextFactory.Create(dbName: dbName))
        {
            seed.Clients.AddRange(
                new Client { CodeClient = "CL00001", NomClient = "Tenant 1", CodeDevise = 1, CompanyId = 1 },
                new Client { CodeClient = "CL00002", NomClient = "Tenant 2", CodeDevise = 1, CompanyId = 2 });
            await seed.SaveChangesAsync();
        }

        var session = TenantSession(companyId: 1);
        using var context = DbContextFactory.Create(new DesktopExecutionContext(session), dbName);

        // Act
        var visible = await context.Clients.OrderBy(c => c.CodeClient).ToListAsync();

        // Assert
        Assert.Single(visible);
        Assert.Equal("CL00001", visible[0].CodeClient);
    }

    [Fact]
    public async Task QueryFilter_WhenSuperAdmin_BypassesTenantFilter()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using (var seed = DbContextFactory.Create(dbName: dbName))
        {
            seed.Clients.AddRange(
                new Client { CodeClient = "CL00001", NomClient = "Tenant 1", CodeDevise = 1, CompanyId = 1 },
                new Client { CodeClient = "CL00002", NomClient = "Tenant 2", CodeDevise = 1, CompanyId = 2 });
            await seed.SaveChangesAsync();
        }

        var session = SuperAdminSession();
        using var context = DbContextFactory.Create(new DesktopExecutionContext(session), dbName);

        // Act
        var visible = await context.Clients.OrderBy(c => c.CodeClient).ToListAsync();

        // Assert
        Assert.Equal(2, visible.Count);
    }

    private static UserSession TenantSession(int companyId)
    {
        var session = new UserSession();
        session.SignIn(userId: 1, companyId: companyId, login: "u", role: "Employé", isSuperAdmin: false);
        return session;
    }

    private static UserSession SuperAdminSession()
    {
        var session = new UserSession();
        session.SignIn(userId: 1, companyId: null, login: "root", role: "SuperAdmin", isSuperAdmin: true);
        return session;
    }
}
