using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;
using Web_T4C_GestCom.Tests.Helpers;
using Xunit;

namespace Web_T4C_GestCom.Tests.Services;

public class CompanyServiceTests
{
    private static CompanyService CreateService(out AppDbContext db)
    {
        db = DbContextFactory.Create();
        return new CompanyService(db);
    }

    [Fact]
    public async Task AddAsync_PersistsNewCompany()
    {
        var svc = CreateService(out var db);
        var baseline = await db.Companies.CountAsync();

        await svc.AddAsync(new Company { Name = "Société Alpha", Slug = "alpha", Plan = "Standard" });

        Assert.Equal(baseline + 1, await db.Companies.CountAsync());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCompaniesOrderedByName()
    {
        var svc = CreateService(out var db);
        db.Companies.Add(new Company { Name = "Zeta SARL" });
        db.Companies.Add(new Company { Name = "01 - Alpha SARL" });
        await db.SaveChangesAsync();

        var result = await svc.GetAllAsync();
        var names = result.Select(c => c.Name).ToList();

        // "01 - Alpha SARL" sorts before the seeded "Entreprise Défaut" and "Zeta SARL" after it.
        Assert.Equal("01 - Alpha SARL", names.First());
        Assert.Equal("Zeta SARL", names.Last());
    }

    [Fact]
    public async Task GetAllAsync_IncludesUserCountForEachCompany()
    {
        var svc = CreateService(out var db);
        var company = new Company { Name = "Alpha SARL" };
        db.Companies.Add(company);
        await db.SaveChangesAsync();
        db.Utilisateurs.Add(new Utilisateur
        {
            Login = "user1", Prenom = "U", Nom = "1", Role = "Employé", CompanyId = company.Id,
            PasswordHash = "x", SecurityStamp = "x"
        });
        await db.SaveChangesAsync();

        var result = await svc.GetAllAsync();

        Assert.Single(result.Single(c => c.Id == company.Id).Utilisateurs);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChangedFields()
    {
        var svc = CreateService(out var db);
        var company = new Company { Name = "Alpha SARL", Plan = "Standard" };
        db.Companies.Add(company);
        await db.SaveChangesAsync();
        db.Entry(company).State = EntityState.Detached;

        company.Name = "Alpha SARL (renommée)";
        company.Plan = "Pro";
        await svc.UpdateAsync(company);

        var reloaded = await db.Companies.AsNoTracking().SingleAsync(c => c.Id == company.Id);
        Assert.Equal("Alpha SARL (renommée)", reloaded.Name);
        Assert.Equal("Pro", reloaded.Plan);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCompany()
    {
        var svc = CreateService(out var db);
        var baseline = await db.Companies.CountAsync();
        var company = new Company { Name = "Alpha SARL" };
        db.Companies.Add(company);
        await db.SaveChangesAsync();
        db.Entry(company).State = EntityState.Detached;

        await svc.DeleteAsync(company);

        Assert.Equal(baseline, await db.Companies.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_AfterEarlierAddOnSameContext_DoesNotThrowTrackingConflict()
    {
        // Same Blazor Server long-lived-DbContext scenario fixed for ReferenceDataService — a
        // Company added earlier in the circuit stays tracked, so a later Delete via a fresh
        // AsNoTracking() instance with the same key must not throw a tracking-conflict exception.
        var svc = CreateService(out var db);
        var added = new Company { Name = "Alpha SARL" };
        await svc.AddAsync(added);

        var loaded = await db.Companies.AsNoTracking().SingleAsync(c => c.Id == added.Id);

        await svc.DeleteAsync(loaded);

        Assert.False(await db.Companies.AnyAsync(c => c.Id == added.Id));
    }

    [Fact]
    public void CompanyToUtilisateurForeignKey_UsesGlobalRestrict()
    {
        using var db = DbContextFactory.Create();

        var fk = db.Model.FindEntityType(typeof(Utilisateur))!
            .GetForeignKeys()
            .Single(f => f.PrincipalEntityType.ClrType == typeof(Company));

        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }
}
