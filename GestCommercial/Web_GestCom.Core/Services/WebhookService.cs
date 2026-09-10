using Microsoft.EntityFrameworkCore;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;

namespace Web_GestCom.Services;

/// <summary>
/// CRUD for Webhook — SuperAdmin-only platform-integration storage. Management page only: no
/// delivery mechanism is wired to these records (see Webhook doc comment).
/// </summary>
public interface IWebhookService
{
    Task<List<Webhook>> GetAllAsync();
    Task AddAsync(Webhook webhook);
    Task UpdateAsync(Webhook webhook);
    Task DeleteAsync(Webhook webhook);
}

public class WebhookService(AppDbContext db) : IWebhookService
{
    public async Task<List<Webhook>> GetAllAsync()
        => await db.Webhooks.AsNoTracking()
            .Include(w => w.Company)
            .OrderByDescending(w => w.DateCreation)
            .ToListAsync();

    public async Task AddAsync(Webhook webhook)
    {
        webhook.DateCreation = DateTime.UtcNow;
        db.Webhooks.Add(webhook);
        await db.SaveChangesGuardedAsync();
    }

    public async Task UpdateAsync(Webhook webhook)
    {
        db.DetachStaleTrackedEntry(webhook);
        db.Webhooks.Update(webhook);
        await db.SaveChangesGuardedAsync();
    }

    public async Task DeleteAsync(Webhook webhook)
    {
        db.DetachStaleTrackedEntry(webhook);
        db.Webhooks.Remove(webhook);
        await db.SaveChangesGuardedAsync();
    }
}
