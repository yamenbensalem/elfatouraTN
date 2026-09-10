using Microsoft.EntityFrameworkCore;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;
using Web_GestCom.Tests.Helpers;
using Xunit;

namespace Web_GestCom.Tests.Services;

public class WebhookServiceTests
{
    private static WebhookService CreateService(out AppDbContext db)
    {
        db = DbContextFactory.Create();
        return new WebhookService(db);
    }

    [Fact]
    public async Task AddAsync_PersistsNewWebhook()
    {
        var svc = CreateService(out var db);
        var baseline = await db.Webhooks.CountAsync();

        await svc.AddAsync(new Webhook { Name = "Hook Test", Url = "https://example.com/hook", EventType = "facture.creee" });

        Assert.Equal(baseline + 1, await db.Webhooks.CountAsync());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsWebhooksAcrossAllCompanies()
    {
        var svc = CreateService(out var db);
        var c1 = new Company { Name = "Alpha" };
        db.Companies.Add(c1);
        await db.SaveChangesAsync();
        db.Webhooks.AddRange(
            new Webhook { Name = "Hook Alpha", Url = "https://a.example.com", CompanyId = c1.Id },
            new Webhook { Name = "Hook Plateforme", Url = "https://b.example.com" });
        await db.SaveChangesAsync();

        var result = await svc.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChangedFields()
    {
        var svc = CreateService(out var db);
        var hook = new Webhook { Name = "Hook Test", Url = "https://old.example.com", Active = true };
        db.Webhooks.Add(hook);
        await db.SaveChangesAsync();
        db.Entry(hook).State = EntityState.Detached;

        hook.Url = "https://new.example.com";
        hook.Active = false;
        await svc.UpdateAsync(hook);

        var reloaded = await db.Webhooks.AsNoTracking().SingleAsync(w => w.Id == hook.Id);
        Assert.Equal("https://new.example.com", reloaded.Url);
        Assert.False(reloaded.Active);
    }

    [Fact]
    public async Task DeleteAsync_RemovesWebhook()
    {
        var svc = CreateService(out var db);
        var hook = new Webhook { Name = "Hook Test", Url = "https://example.com" };
        db.Webhooks.Add(hook);
        await db.SaveChangesAsync();
        db.Entry(hook).State = EntityState.Detached;

        await svc.DeleteAsync(hook);

        Assert.False(await db.Webhooks.AnyAsync(w => w.Id == hook.Id));
    }
}
