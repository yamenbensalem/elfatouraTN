using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;

namespace Web_T4C_GestCom.Services;

public interface IFournisseurService
{
    Task<List<Fournisseur>> GetAllAsync(string? search = null);
    Task<Fournisseur?> GetByCodeAsync(string code);
    Task<string> AddAsync(Fournisseur fournisseur);
    Task UpdateAsync(Fournisseur fournisseur);
    Task DeleteAsync(string code);
}

public class FournisseurService(
    AppDbContext db,
    IJournalActiviteService journal,
    ICurrentUserService? currentUser = null,
    IPermissionService? permissionService = null) : IFournisseurService
{
    public async Task<List<Fournisseur>> GetAllAsync(string? search = null)
    {
        var query = db.Fournisseurs.AsNoTracking().Include(f => f.Devise).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(f =>
                f.NomFournisseur.Contains(search) ||
                f.CodeFournisseur.Contains(search) ||
                (f.MatriculeFiscale != null && f.MatriculeFiscale.Contains(search)));

        return await query.OrderBy(f => f.NomFournisseur).ToListAsync();
    }

    public async Task<Fournisseur?> GetByCodeAsync(string code)
        => await db.Fournisseurs.AsNoTracking().Include(f => f.Devise).FirstOrDefaultAsync(f => f.CodeFournisseur == code);

    public async Task<string> AddAsync(Fournisseur fournisseur)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "fournisseurs.create");

        if (string.IsNullOrWhiteSpace(fournisseur.CodeFournisseur))
        {
            var count = await db.Fournisseurs.CountAsync();
            fournisseur.CodeFournisseur = $"FO{(count + 1):D5}";
        }
        db.Fournisseurs.Add(fournisseur);
        await db.SaveChangesGuardedAsync();
        await journal.EnregistrerAsync("Ajout", "Fournisseur", fournisseur.CodeFournisseur, fournisseur.NomFournisseur);
        return fournisseur.CodeFournisseur;
    }

    public async Task UpdateAsync(Fournisseur fournisseur)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "fournisseurs.update");

        db.Fournisseurs.Update(fournisseur);
        await db.SaveChangesGuardedAsync();
        await journal.EnregistrerAsync("Modification", "Fournisseur", fournisseur.CodeFournisseur, fournisseur.NomFournisseur);
    }

    public async Task DeleteAsync(string code)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "fournisseurs.delete");

        var f = await db.Fournisseurs.FindAsync(code);
        if (f is not null)
        {
            db.Fournisseurs.Remove(f);
            await db.SaveChangesGuardedAsync();
            await journal.EnregistrerAsync("Suppression", "Fournisseur", code, f.NomFournisseur);
        }
    }
}
