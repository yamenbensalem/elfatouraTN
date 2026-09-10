using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Web_GestCom.Auth;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;
using Xunit;

namespace Web_GestCom.Tests.Data;

public class AppDbContextTenantIsolationTests
{
    [Fact]
    public async Task SaveChangesAsync_WhenAddingTenantOwnedEntity_ShouldStampCurrentCompanyId()
    {
        // Arrange
        var options = CreateOptions();
        using var context = CreateTenantContext(options, companyId: 1);

        var client = new Client
        {
            CodeClient = "CL00001",
            NomClient = "Client Tenant 1",
            CodeDevise = 1
        };

        // Act
        context.Clients.Add(client);
        await context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, client.CompanyId);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenCrossTenantModification_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var options = CreateOptions();
        using var context = CreateTenantContext(options, companyId: 1);

        var rogueClient = new Client
        {
            CodeClient = "CL99999",
            NomClient = "Cross Tenant",
            CodeDevise = 1,
            CompanyId = 2
        };

        // Act
        context.Clients.Update(rogueClient);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task QueryFilter_WhenTenantUser_ShouldReturnOnlyCurrentTenantRows()
    {
        // Arrange
        var options = CreateOptions();

        await using (var seed = new AppDbContext(options))
        {
            seed.Clients.AddRange(
                new Client { CodeClient = "CL00001", NomClient = "Tenant 1", CodeDevise = 1, CompanyId = 1 },
                new Client { CodeClient = "CL00002", NomClient = "Tenant 2", CodeDevise = 1, CompanyId = 2 }
            );
            await seed.SaveChangesAsync();
        }

        await using var context = CreateTenantContext(options, companyId: 1);

        // Act
        var visibleClients = await context.Clients.OrderBy(c => c.CodeClient).ToListAsync();

        // Assert
        Assert.Single(visibleClients);
        Assert.Equal("CL00001", visibleClients[0].CodeClient);
    }

    [Fact]
    public async Task QueryFilter_WhenSuperAdmin_ShouldSeeNoBusinessData()
    {
        // Arrange — SuperAdmin is a platform-management role with zero business-data access by
        // design: it must not see Client (or any other business entity) rows from any company,
        // unlike the Utilisateur query filter which deliberately does let it see all companies'
        // users (platform scope, not business data — see ShouldApplyTenantFilterToAuthenticatedUsers).
        var options = CreateOptions();

        await using (var seed = new AppDbContext(options))
        {
            seed.Clients.AddRange(
                new Client { CodeClient = "CL00001", NomClient = "Tenant 1", CodeDevise = 1, CompanyId = 1 },
                new Client { CodeClient = "CL00002", NomClient = "Tenant 2", CodeDevise = 1, CompanyId = 2 }
            );
            await seed.SaveChangesAsync();
        }

        await using var context = CreateTenantContext(options, companyId: null, isSuperAdmin: true);

        // Act
        var visibleClients = await context.Clients.OrderBy(c => c.CodeClient).ToListAsync();

        // Assert
        Assert.Empty(visibleClients);
    }

    private static DbContextOptions<AppDbContext> CreateOptions()
        => new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static AppDbContext CreateTenantContext(
        DbContextOptions<AppDbContext> options,
        int? companyId,
        bool isSuperAdmin = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "tenant-user"),
            new("IsSuperAdmin", isSuperAdmin ? "1" : "0")
        };

        if (companyId.HasValue)
            claims.Add(new Claim("CompanyId", companyId.Value.ToString()));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Tests"));
        var httpContext = new DefaultHttpContext { User = principal };
        var accessor = new HttpContextAccessor { HttpContext = httpContext };
        var executionContext = new HttpExecutionContext(accessor);

        var context = new AppDbContext(options, executionContext);
        context.Database.EnsureCreated();
        return context;
    }
}
