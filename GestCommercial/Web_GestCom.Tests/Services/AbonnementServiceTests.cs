using Microsoft.EntityFrameworkCore;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;
using Web_GestCom.Tests.Helpers;
using Xunit;

namespace Web_GestCom.Tests.Services;

public class AbonnementServiceTests
{
    private static AbonnementService CreateService(out AppDbContext db)
    {
        db = DbContextFactory.Create();
        return new AbonnementService(db);
    }

    [Fact]
    public async Task CreateDemandeAsync_StoresRequestAsPending()
    {
        var svc = CreateService(out var db);

        await svc.CreateDemandeAsync(new Abonnement
        {
            NomEntreprise = "Société Test",
            NomContact = "Karim Ben Ali",
            EmailContact = "Karim@Test.com",
            Plan = "Pro"
        });

        var stored = await db.Abonnements.SingleAsync();
        Assert.Equal("EnAttente", stored.Statut);
        Assert.Null(stored.CompanyId);
        Assert.Null(stored.DateDebut);
    }

    [Fact]
    public async Task CreateDemandeAsync_IgnoresCallerSuppliedStatutAndCompanyId()
    {
        var svc = CreateService(out var db);
        var company = new Company { Name = "Alpha" };
        db.Companies.Add(company);
        await db.SaveChangesAsync();

        await svc.CreateDemandeAsync(new Abonnement
        {
            NomEntreprise = "Société Test",
            NomContact = "Karim",
            EmailContact = "karim@test.com",
            Plan = "Standard",
            Statut = "Active",
            CompanyId = company.Id
        });

        var stored = await db.Abonnements.SingleAsync();
        Assert.Equal("EnAttente", stored.Statut);
        Assert.Null(stored.CompanyId);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsRequestsAcrossAllCompanies_OrderedByMostRecent()
    {
        var svc = CreateService(out var db);
        db.Abonnements.AddRange(
            new Abonnement { NomEntreprise = "Ancienne", NomContact = "A", EmailContact = "a@test.com", Plan = "Standard", DateDemande = DateTime.UtcNow.AddDays(-2) },
            new Abonnement { NomEntreprise = "Récente", NomContact = "B", EmailContact = "b@test.com", Plan = "Pro", DateDemande = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var result = await svc.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Récente", result[0].NomEntreprise);
    }

    [Fact]
    public async Task UpdateAsync_ActivatesRequestAndLinksCompany()
    {
        var svc = CreateService(out var db);
        var company = new Company { Name = "Beta" };
        db.Companies.Add(company);
        var demande = new Abonnement { NomEntreprise = "Beta", NomContact = "C", EmailContact = "c@test.com", Plan = "Pro" };
        db.Abonnements.Add(demande);
        await db.SaveChangesAsync();
        db.Entry(demande).State = EntityState.Detached;
        db.Entry(company).State = EntityState.Detached;

        demande.Statut = "Active";
        demande.CompanyId = company.Id;
        demande.DateDebut = DateTime.UtcNow;
        await svc.UpdateAsync(demande);

        var stored = await db.Abonnements.AsNoTracking().SingleAsync();
        Assert.Equal("Active", stored.Statut);
        Assert.Equal(company.Id, stored.CompanyId);
        Assert.NotNull(stored.DateDebut);
    }

    [Fact]
    public async Task DeleteAsync_RemovesRequest()
    {
        var svc = CreateService(out var db);
        var demande = new Abonnement { NomEntreprise = "Gamma", NomContact = "D", EmailContact = "d@test.com", Plan = "Standard" };
        db.Abonnements.Add(demande);
        await db.SaveChangesAsync();
        db.Entry(demande).State = EntityState.Detached;

        await svc.DeleteAsync(demande);

        Assert.False(await db.Abonnements.AnyAsync(a => a.Id == demande.Id));
    }
}
