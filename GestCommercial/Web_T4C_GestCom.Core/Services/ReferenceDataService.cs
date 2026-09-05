using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;

namespace Web_T4C_GestCom.Services;

/// <summary>
/// Generic CRUD for flat reference-data lookup tables (TvaProduit, CategorieProduit, UniteProduit,
/// ModePayement, Devise, FabriquantProduit) — none of them carry navigation properties or business
/// logic beyond "list/add/update/delete a row", so one generic service replaces 6 near-identical ones.
/// Not meant for entities with relationships or invariants — those get their own dedicated service.
/// </summary>
public interface IReferenceDataService<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}

public class ReferenceDataService<T>(AppDbContext db) : IReferenceDataService<T> where T : class
{
    public async Task<List<T>> GetAllAsync()
        => await db.Set<T>().AsNoTracking().ToListAsync();

    public async Task AddAsync(T entity)
    {
        db.Set<T>().Add(entity);
        await db.SaveChangesGuardedAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        db.DetachStaleTrackedEntry(entity);
        db.Set<T>().Update(entity);
        await db.SaveChangesGuardedAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        db.DetachStaleTrackedEntry(entity);
        db.Set<T>().Remove(entity);
        await db.SaveChangesGuardedAsync();
    }
}
