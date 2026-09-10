using Microsoft.EntityFrameworkCore;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;
using Web_GestCom.Tests.Helpers;
using Xunit;

namespace Web_GestCom.Tests.Services;

public class ApiKeyServiceTests
{
    private static ApiKeyService CreateService(out AppDbContext db)
    {
        db = DbContextFactory.Create();
        return new ApiKeyService(db);
    }

    [Fact]
    public async Task AddAsync_WithoutExplicitValue_GeneratesOne()
    {
        var svc = CreateService(out var db);

        await svc.AddAsync(new ApiKey { Name = "Clé Test" });

        var stored = await db.ApiKeys.SingleAsync();
        Assert.False(string.IsNullOrWhiteSpace(stored.Value));
        Assert.StartsWith("gc_", stored.Value, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AddAsync_PlatformWide_LeavesCompanyIdNull()
    {
        var svc = CreateService(out var db);

        await svc.AddAsync(new ApiKey { Name = "Clé Plateforme" });

        var stored = await db.ApiKeys.SingleAsync();
        Assert.Null(stored.CompanyId);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsKeysAcrossAllCompanies()
    {
        var svc = CreateService(out var db);
        var c1 = new Company { Name = "Alpha" };
        var c2 = new Company { Name = "Beta" };
        db.Companies.AddRange(c1, c2);
        await db.SaveChangesAsync();
        db.ApiKeys.AddRange(
            new ApiKey { Name = "Clé Alpha", Value = "v1", CompanyId = c1.Id },
            new ApiKey { Name = "Clé Beta", Value = "v2", CompanyId = c2.Id });
        await db.SaveChangesAsync();

        var result = await svc.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task DeleteAsync_RemovesApiKey()
    {
        var svc = CreateService(out var db);
        var key = new ApiKey { Name = "Clé Test", Value = "v1" };
        db.ApiKeys.Add(key);
        await db.SaveChangesAsync();
        db.Entry(key).State = EntityState.Detached;

        await svc.DeleteAsync(key);

        Assert.False(await db.ApiKeys.AnyAsync(k => k.Id == key.Id));
    }
}
