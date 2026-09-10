using Microsoft.EntityFrameworkCore;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;

namespace Web_GestCom.Services;

/// <summary>
/// CRUD for Company (tenant) records — SuperAdmin-only. Regular users stay pinned to the single
/// company assigned at creation (Utilisateur.CompanyId); this service only lets a SuperAdmin
/// create/list/rename/delete the companies themselves, not manage a user's membership across
/// several of them.
/// </summary>
public interface ICompanyService
{
    Task<List<Company>> GetAllAsync();
    Task<Company?> GetByIdAsync(int id);
    Task AddAsync(Company company);
    Task UpdateAsync(Company company);
    Task DeleteAsync(Company company);
}

public class CompanyService(AppDbContext db) : ICompanyService
{
    public async Task<List<Company>> GetAllAsync()
        => await db.Companies.AsNoTracking()
            .Include(c => c.Utilisateurs)
            .OrderBy(c => c.Name)
            .ToListAsync();

    public async Task<Company?> GetByIdAsync(int id)
        => await db.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public async Task AddAsync(Company company)
    {
        ApplyDefaultQuotasIfUnset(company);
        db.Companies.Add(company);
        await db.SaveChangesGuardedAsync();
    }

    /// <summary>
    /// Seeds the per-role self-service user quotas from the company's Plan the first time it's
    /// created, unless the caller already set them explicitly. Purely a starting point: a
    /// SuperAdmin can freely edit these afterward (UpdateAsync never touches them), and a null
    /// value always means "illimité" regardless of Plan.
    /// </summary>
    private static void ApplyDefaultQuotasIfUnset(Company company)
    {
        if (company.MaxAdmins.HasValue || company.MaxManagers.HasValue || company.MaxEmployes.HasValue)
            return;

        (company.MaxAdmins, company.MaxManagers, company.MaxEmployes) = company.Plan switch
        {
            "Pro" => (2, 3, 10),
            "Enterprise" => (2, 3, 10), // Aligné sur Pro pour l'instant — à ajuster au cas par cas.
            _ => (1, 1, 2) // Standard (et tout Plan inconnu) — quota d'origine demandé.
        };
    }

    public async Task UpdateAsync(Company company)
    {
        db.DetachStaleTrackedEntry(company);
        db.Companies.Update(company);
        await db.SaveChangesGuardedAsync();
    }

    public async Task DeleteAsync(Company company)
    {
        db.DetachStaleTrackedEntry(company);
        db.Companies.Remove(company);
        await db.SaveChangesGuardedAsync();
    }
}
