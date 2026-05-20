using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;

namespace Web_T4C_GestCom.Services;

public interface IFactureFournisseurService
{
    Task<List<FactureFournisseur>> GetAllAsync();
    Task<FactureFournisseur?> GetByNumeroAsync(string numero);
    Task<FactureFournisseur> CreateAsync(FactureFournisseur facture, List<LigneFactureFournisseur> lignes);
    Task<FactureFournisseur> UpdateAsync(FactureFournisseur facture, List<LigneFactureFournisseur> lignes);
    Task DeleteAsync(string numero);
    Task<FactureFournisseur> CloneAsync(string numero);
    Task AddReglementAsync(ReglementFactureFournisseur reglement);
    Task<double> GetSoldeAsync(string numero);
}

public class FactureFournisseurService(
    AppDbContext db,
    DocumentNumberService numService,
    IJournalActiviteService journal,
    ICurrentUserService? currentUser = null,
    IPermissionService? permissionService = null)
    : IFactureFournisseurService
{
    public async Task<List<FactureFournisseur>> GetAllAsync()
        => await db.FacturesFournisseur
            .Include(f => f.Fournisseur)
            .OrderByDescending(f => f.DateFactureFournisseur)
            .ToListAsync();

    public async Task<FactureFournisseur?> GetByNumeroAsync(string numero)
        => await db.FacturesFournisseur
            .Include(f => f.Fournisseur)
            .Include(f => f.Lignes).ThenInclude(l => l.Produit)
            .Include(f => f.Reglements).ThenInclude(r => r.ModePayement)
            .FirstOrDefaultAsync(f => f.NumeroFactureFournisseur == numero);

    public async Task<FactureFournisseur> CreateAsync(FactureFournisseur facture, List<LigneFactureFournisseur> lignes)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "factures-fournisseur.create");

        facture.NumeroFactureFournisseur = await numService.NextFactureFournisseurAsync();
        RecalculateTotals(facture, lignes);
        facture.Lignes = lignes;

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            db.FacturesFournisseur.Add(facture);
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

        await journal.EnregistrerAsync("Ajout", "FactureFournisseur", facture.NumeroFactureFournisseur, facture.CodeFournisseur);
        return facture;
    }

    public async Task<FactureFournisseur> UpdateAsync(FactureFournisseur facture, List<LigneFactureFournisseur> lignes)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "factures-fournisseur.update");

        var existing = await db.FacturesFournisseur
            .Include(f => f.Lignes)
            .FirstOrDefaultAsync(f => f.NumeroFactureFournisseur == facture.NumeroFactureFournisseur)
            ?? throw new InvalidOperationException("Facture fournisseur introuvable.");

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            foreach (var l in existing.Lignes)
                await ApplyStockDeltaAsync(l.CodeProduit, -l.Quantite);
            db.LignesFactureFournisseur.RemoveRange(existing.Lignes);
            await db.SaveChangesAsync();

            existing.DateFactureFournisseur = facture.DateFactureFournisseur;
            existing.CodeFournisseur = facture.CodeFournisseur;
            existing.Timbre = facture.Timbre;
            existing.EtatFacture = facture.EtatFacture;
            existing.Note = facture.Note;

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

        await journal.EnregistrerAsync("Modification", "FactureFournisseur", existing.NumeroFactureFournisseur, existing.CodeFournisseur);
        return existing;
    }

    public async Task DeleteAsync(string numero)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "factures-fournisseur.delete");

        var facture = await db.FacturesFournisseur
            .Include(f => f.Lignes)
            .Include(f => f.Reglements)
            .FirstOrDefaultAsync(f => f.NumeroFactureFournisseur == numero)
            ?? throw new InvalidOperationException("Facture fournisseur introuvable.");

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            foreach (var l in facture.Lignes)
                await ApplyStockDeltaAsync(l.CodeProduit, -l.Quantite);
            db.ReglementsFactureFournisseur.RemoveRange(facture.Reglements);
            db.LignesFactureFournisseur.RemoveRange(facture.Lignes);
            db.FacturesFournisseur.Remove(facture);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        await journal.EnregistrerAsync("Suppression", "FactureFournisseur", numero);
    }

    public async Task<FactureFournisseur> CloneAsync(string numero)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "factures-fournisseur.create");

        var source = await GetByNumeroAsync(numero)
            ?? throw new InvalidOperationException("Facture fournisseur introuvable.");

        var clone = new FactureFournisseur
        {
            NumeroFactureFournisseur = await numService.NextFactureFournisseurAsync(),
            DateFactureFournisseur = DateTime.Today,
            CodeFournisseur = source.CodeFournisseur,
            Timbre = source.Timbre,
            EtatFacture = "Facture Ouverte",
            EtatReglement = "Non Réglé",
            Note = source.Note
        };

        var lignes = source.Lignes.Select(l => new LigneFactureFournisseur
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
            db.FacturesFournisseur.Add(clone);
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

        await journal.EnregistrerAsync("Clone", "FactureFournisseur", clone.NumeroFactureFournisseur, $"cloné depuis {numero}");
        return clone;
    }

    public async Task AddReglementAsync(ReglementFactureFournisseur reglement)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "factures-fournisseur.update");

        reglement.Id = 0;
        db.ReglementsFactureFournisseur.Add(reglement);
        await db.SaveChangesAsync();
        await UpdateEtatReglementAsync(reglement.NumeroFactureFournisseur);
    }

    public async Task<double> GetSoldeAsync(string numero)
    {
        var facture = await db.FacturesFournisseur
            .Include(f => f.Reglements)
            .FirstOrDefaultAsync(f => f.NumeroFactureFournisseur == numero);
        if (facture is null) return 0;
        var totalRegle = facture.Reglements.Sum(r => r.Montant);
        return Math.Round(facture.MontantTTC + facture.Timbre - totalRegle, 3);
    }

    private async Task ApplyStockDeltaAsync(string codeProduit, double delta)
    {
        var produit = await db.Produits.FindAsync(codeProduit)
            ?? throw new InvalidOperationException($"Produit {codeProduit} introuvable.");
        produit.Quantite += delta;
    }

    private async Task UpdateEtatReglementAsync(string numero)
    {
        var facture = await db.FacturesFournisseur
            .Include(f => f.Reglements)
            .FirstOrDefaultAsync(f => f.NumeroFactureFournisseur == numero);
        if (facture is null) return;

        var netAPayer = facture.MontantTTC + facture.Timbre;
        var totalRegle = facture.Reglements.Sum(r => r.Montant);

        facture.EtatReglement = totalRegle <= 0 ? "Non Réglé"
            : totalRegle >= netAPayer ? "Réglé"
            : "Partiellement Réglé";

        await db.SaveChangesAsync();
    }

    private static void RecalculateTotals(FactureFournisseur doc, List<LigneFactureFournisseur> lignes)
    {
        doc.MontantHT = lignes.Sum(l => l.MontantHT);
        doc.MontantTVA = Math.Round(lignes.Sum(l => l.MontantHT * l.Tva / 100.0), 3);
        doc.MontantTTC = Math.Round(doc.MontantHT + doc.MontantTVA, 3);
    }
}
