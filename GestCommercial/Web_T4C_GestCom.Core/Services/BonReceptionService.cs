using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;

namespace Web_T4C_GestCom.Services;

public interface IBonReceptionService
{
    Task<List<BonReception>> GetAllAsync();
    Task<BonReception?> GetByNumeroAsync(string numero);
    Task<BonReception> CreateAsync(BonReception bon, List<LigneBonReception> lignes);
    Task<BonReception> UpdateAsync(BonReception bon, List<LigneBonReception> lignes);
    Task DeleteAsync(string numero);
    Task<BonReception> CloneAsync(string numero);
}

public class BonReceptionService(
    AppDbContext db,
    DocumentNumberService numService,
    IJournalActiviteService journal,
    ICurrentUserService? currentUser = null,
    IPermissionService? permissionService = null)
    : IBonReceptionService
{
    public async Task<List<BonReception>> GetAllAsync()
        => await db.BonsReception
            .Include(b => b.Fournisseur)
            .Include(b => b.CommandeAchat)
            .OrderByDescending(b => b.DateBonReception)
            .ToListAsync();

    public async Task<BonReception?> GetByNumeroAsync(string numero)
        => await db.BonsReception
            .Include(b => b.Fournisseur)
            .Include(b => b.CommandeAchat)
            .Include(b => b.Lignes).ThenInclude(l => l.Produit)
            .FirstOrDefaultAsync(b => b.NumeroBonReception == numero);

    public async Task<BonReception> CreateAsync(BonReception bon, List<LigneBonReception> lignes)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "bons-reception.create");

        bon.NumeroBonReception = await numService.NextBonReceptionAsync();
        RecalculateTotals(bon, lignes);
        bon.Lignes = lignes;

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            db.BonsReception.Add(bon);
            foreach (var l in lignes)
                await ApplyStockDeltaAsync(l.CodeProduit, +l.Quantite);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        await journal.EnregistrerAsync("Ajout", "BonReception", bon.NumeroBonReception, bon.CodeFournisseur);
        return bon;
    }

    public async Task<BonReception> UpdateAsync(BonReception bon, List<LigneBonReception> lignes)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "bons-reception.update");

        var existing = await db.BonsReception
            .Include(b => b.Lignes)
            .FirstOrDefaultAsync(b => b.NumeroBonReception == bon.NumeroBonReception)
            ?? throw new InvalidOperationException("Bon de réception introuvable.");

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            foreach (var l in existing.Lignes)
                await ApplyStockDeltaAsync(l.CodeProduit, -l.Quantite);
            db.LignesBonReception.RemoveRange(existing.Lignes);
            await db.SaveChangesAsync();

            existing.DateBonReception = bon.DateBonReception;
            existing.CodeFournisseur = bon.CodeFournisseur;
            existing.NumeroCommandeAchat = bon.NumeroCommandeAchat;
            existing.EtatBonReception = bon.EtatBonReception;
            existing.EtatFacture = bon.EtatFacture;
            existing.Note = bon.Note;

            foreach (var l in lignes) l.Id = 0;
            RecalculateTotals(existing, lignes);
            existing.Lignes = lignes;

            foreach (var l in lignes)
                await ApplyStockDeltaAsync(l.CodeProduit, +l.Quantite);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        await journal.EnregistrerAsync("Modification", "BonReception", existing.NumeroBonReception, existing.CodeFournisseur);
        return existing;
    }

    public async Task DeleteAsync(string numero)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "bons-reception.delete");

        var bon = await db.BonsReception
            .Include(b => b.Lignes)
            .FirstOrDefaultAsync(b => b.NumeroBonReception == numero)
            ?? throw new InvalidOperationException("Bon de réception introuvable.");

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            foreach (var l in bon.Lignes)
                await ApplyStockDeltaAsync(l.CodeProduit, -l.Quantite);
            db.LignesBonReception.RemoveRange(bon.Lignes);
            db.BonsReception.Remove(bon);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        await journal.EnregistrerAsync("Suppression", "BonReception", numero);
    }

    public async Task<BonReception> CloneAsync(string numero)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "bons-reception.create");

        var source = await GetByNumeroAsync(numero)
            ?? throw new InvalidOperationException("Bon de réception introuvable.");

        var clone = new BonReception
        {
            NumeroBonReception = await numService.NextBonReceptionAsync(),
            DateBonReception = DateTime.Today,
            CodeFournisseur = source.CodeFournisseur,
            NumeroCommandeAchat = source.NumeroCommandeAchat,
            EtatBonReception = "Ouvert",
            EtatFacture = "Non Facturé",
            Note = source.Note
        };

        var lignes = source.Lignes.Select(l => new LigneBonReception
        {
            Id = 0,
            CodeProduit = l.CodeProduit,
            Quantite = l.Quantite,
            PrixUnitaire = l.PrixUnitaire,
            Tva = l.Tva,
            MontantHT = l.MontantHT
        }).ToList();

        RecalculateTotals(clone, lignes);
        clone.Lignes = lignes;

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            db.BonsReception.Add(clone);
            foreach (var l in lignes)
                await ApplyStockDeltaAsync(l.CodeProduit, +l.Quantite);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        await journal.EnregistrerAsync("Clone", "BonReception", clone.NumeroBonReception, $"cloné depuis {numero}");
        return clone;
    }

    private async Task ApplyStockDeltaAsync(string codeProduit, double delta)
    {
        var produit = await db.Produits.FindAsync(codeProduit)
            ?? throw new InvalidOperationException($"Produit {codeProduit} introuvable.");
        produit.Quantite += delta;
    }

    private static void RecalculateTotals(BonReception doc, List<LigneBonReception> lignes)
    {
        doc.MontantHT = lignes.Sum(l => l.MontantHT);
        doc.MontantTVA = Math.Round(lignes.Sum(l => l.MontantHT * l.Tva / 100.0), 3);
        doc.MontantTTC = Math.Round(doc.MontantHT + doc.MontantTVA, 3);
    }
}
