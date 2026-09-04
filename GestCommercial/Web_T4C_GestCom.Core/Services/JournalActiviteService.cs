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
}
