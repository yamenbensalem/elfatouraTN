using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;

namespace Web_T4C_GestCom.Services;

public interface IJournalActiviteService
{
    /// <summary>N'écrase jamais l'exception — le journal ne doit pas crasher l'app.</summary>
    Task EnregistrerAsync(string action, string entite, string? codeEntite = null, string? detail = null);

    Task<List<JournalActivite>> GetAllAsync(
        string? login = null,
        string? entite = null,
        DateTime? debut = null,
        DateTime? fin = null);

    Task<List<string>> GetLoginsDistinctsAsync();
    Task<List<string>> GetEntitesDistinctesAsync();

    /// <summary>
    /// Supprime définitivement les entrées de plus de <paramref name="olderThanMonths"/> mois et
    /// retourne le nombre supprimé. Contrairement à EnregistrerAsync, ne masque pas les erreurs —
    /// c'est une action destructive délibérée déclenchée par un admin, pas un effet de bord silencieux.
    /// </summary>
    Task<int> PurgeAsync(int olderThanMonths);
}

public class JournalActiviteService(
    AppDbContext db,
    ICurrentUserService currentUser,
    ITenantService? tenantService = null) : IJournalActiviteService
{
    public async Task EnregistrerAsync(string action, string entite, string? codeEntite = null, string? detail = null)
    {
        try
        {
            db.JournalActivites.Add(new JournalActivite
            {
                Login      = currentUser.Login,
                Action     = action,
                Entite     = entite,
                CodeEntite = codeEntite,
                DateHeure  = DateTime.Now,
                Detail     = detail,
                CompanyId  = tenantService?.CurrentCompanyId
            });
            await db.SaveChangesGuardedAsync();
        }
        catch
        {
            // Journal silencieux — ne doit jamais interrompre l'opération métier
        }
    }

    public async Task<List<JournalActivite>> GetAllAsync(
        string? login = null,
        string? entite = null,
        DateTime? debut = null,
        DateTime? fin = null)
    {
        var q = db.JournalActivites.AsNoTracking().AsQueryable();

        if (tenantService?.CurrentCompanyId is int companyId)
            q = q.Where(j => j.CompanyId == companyId);

        if (!string.IsNullOrWhiteSpace(login))
            q = q.Where(j => j.Login == login);

        if (!string.IsNullOrWhiteSpace(entite))
            q = q.Where(j => j.Entite == entite);

        if (debut.HasValue)
            q = q.Where(j => j.DateHeure >= debut.Value.Date);

        if (fin.HasValue)
            q = q.Where(j => j.DateHeure < fin.Value.Date.AddDays(1));

        return await q.OrderByDescending(j => j.DateHeure).Take(1000).ToListAsync();
    }

    public async Task<List<string>> GetLoginsDistinctsAsync()
    {
        var q = db.JournalActivites.AsQueryable();
        if (tenantService?.CurrentCompanyId is int companyId)
            q = q.Where(j => j.CompanyId == companyId);

        return await q.Select(j => j.Login).Distinct().OrderBy(l => l).ToListAsync();
    }

    public async Task<List<string>> GetEntitesDistinctesAsync()
    {
        var q = db.JournalActivites.AsQueryable();
        if (tenantService?.CurrentCompanyId is int companyId)
            q = q.Where(j => j.CompanyId == companyId);

        return await q.Select(j => j.Entite).Distinct().OrderBy(e => e).ToListAsync();
    }

    public async Task<int> PurgeAsync(int olderThanMonths)
    {
        if (olderThanMonths <= 0)
            throw new ArgumentOutOfRangeException(nameof(olderThanMonths), "Le seuil doit être d'au moins 1 mois.");

        var cutoff = DateTime.Today.AddMonths(-olderThanMonths);
        var q = db.JournalActivites.Where(j => j.DateHeure < cutoff);
        if (tenantService?.CurrentCompanyId is int companyId)
            q = q.Where(j => j.CompanyId == companyId);

        var toDelete = await q.ToListAsync();
        if (toDelete.Count == 0)
            return 0;

        db.JournalActivites.RemoveRange(toDelete);
        await db.SaveChangesGuardedAsync();

        // Écrit après coup, hors de la sélection déjà supprimée — trace que la purge a eu lieu.
        await EnregistrerAsync("Purge", "JournalActivite", null,
            $"{toDelete.Count} entrée(s) antérieure(s) au {cutoff:dd/MM/yyyy} supprimée(s).");

        return toDelete.Count;
    }
}
