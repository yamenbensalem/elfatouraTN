using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;

namespace Web_T4C_GestCom.Services;

public interface IProduitService
{
    Task<List<Produit>> GetAllAsync(string? search = null, int? categorieCode = null);
    Task<Produit?> GetByCodeAsync(string code);
    Task<string> AddAsync(Produit produit);
    Task UpdateAsync(Produit produit);
    Task DeleteAsync(string code);
    Task UpdateStockAsync(string codeProduit, double delta);
    /// <summary>Applique un delta de stock sur l'entité trackée sans appeler SaveChangesAsync. Le caller est responsable de la persistance.</summary>
    Task ApplyStockDeltaAsync(string codeProduit, double delta);
    Task<List<Produit>> GetStockAlerteAsync();
}

public class ProduitService(
    AppDbContext db,
    IJournalActiviteService journal,
    ICurrentUserService? currentUser = null,
    IPermissionService? permissionService = null) : IProduitService
{
    public async Task<List<Produit>> GetAllAsync(string? search = null, int? categorieCode = null)
    {
        var query = db.Produits
            .AsNoTracking()
            .Include(p => p.Devise)
            .Include(p => p.UniteProduit)
            .Include(p => p.TvaProduit)
            .Include(p => p.CategorieProduit)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.DesignationProduit.Contains(search) ||
                p.CodeProduit.Contains(search));

        if (categorieCode.HasValue)
            query = query.Where(p => p.CodeCategorieProduit == categorieCode.Value);

        return await query.OrderBy(p => p.DesignationProduit).ToListAsync();
    }

    public async Task<Produit?> GetByCodeAsync(string code)
        => await db.Produits
            .AsNoTracking()
            .Include(p => p.Devise)
            .Include(p => p.UniteProduit)
            .Include(p => p.TvaProduit)
            .Include(p => p.CategorieProduit)
            .Include(p => p.FabriquantProduit)
            .FirstOrDefaultAsync(p => p.CodeProduit == code);

    public async Task<string> AddAsync(Produit produit)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "produits.create");

        if (string.IsNullOrWhiteSpace(produit.CodeProduit))
        {
            var count = await db.Produits.CountAsync();
            produit.CodeProduit = $"PR{(count + 1):D5}";
        }
        db.Produits.Add(produit);
        await db.SaveChangesGuardedAsync();
        await journal.EnregistrerAsync("Ajout", "Produit", produit.CodeProduit, produit.DesignationProduit);
        return produit.CodeProduit;
    }

    public async Task UpdateAsync(Produit produit)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "produits.update");

        db.Produits.Update(produit);
        await db.SaveChangesGuardedAsync();
        await journal.EnregistrerAsync("Modification", "Produit", produit.CodeProduit, produit.DesignationProduit);
    }

    public async Task DeleteAsync(string code)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "produits.delete");

        var produit = await db.Produits.FindAsync(code);
        if (produit is not null)
        {
            db.Produits.Remove(produit);
            await db.SaveChangesGuardedAsync();
            await journal.EnregistrerAsync("Suppression", "Produit", code, produit.DesignationProduit);
        }
    }

    public async Task UpdateStockAsync(string codeProduit, double delta)
    {
        var produit = await db.Produits.FindAsync(codeProduit);
        if (produit is not null)
        {
            produit.Quantite += delta;
            await db.SaveChangesGuardedAsync();
        }
    }

    public async Task ApplyStockDeltaAsync(string codeProduit, double delta)
    {
        var produit = await db.Produits.FindAsync(codeProduit);
        if (produit is not null)
            produit.Quantite += delta;
    }

    public async Task<List<Produit>> GetStockAlerteAsync()
        => await db.Produits
            .AsNoTracking()
            .Include(p => p.UniteProduit)
            .Where(p => p.Quantite <= p.StockMinimal)
            .OrderBy(p => p.DesignationProduit)
            .ToListAsync();
}
