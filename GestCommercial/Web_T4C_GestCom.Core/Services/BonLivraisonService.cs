using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;

namespace Web_T4C_GestCom.Services;

public interface IBonLivraisonService
{
    Task<List<BonLivraison>> GetAllAsync(string? clientCode = null);
    Task<BonLivraison?> GetByNumeroAsync(string numero);
    Task<BonLivraison> CreateAsync(BonLivraison bon, List<LigneBonLivraison> lignes);
    Task UpdateAsync(BonLivraison bon, List<LigneBonLivraison> lignes);
    Task DeleteAsync(string numero);
    Task<BonLivraison> CloneAsync(string numero);
    Task<BonLivraison> CreateFromCommandeVenteAsync(string numeroCommandeVente);
}

public class BonLivraisonService(
    AppDbContext db,
    DocumentNumberService numService,
    IProduitService produitService,
    IJournalActiviteService journal,
    ICurrentUserService? currentUser = null,
    IPermissionService? permissionService = null) : IBonLivraisonService
{
    public async Task<List<BonLivraison>> GetAllAsync(string? clientCode = null)
    {
        var query = db.BonsLivraison
            .Include(b => b.Client)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(clientCode))
            query = query.Where(b => b.CodeClient == clientCode);

        return await query.OrderByDescending(b => b.DateBonLivraison).ToListAsync();
    }

    public async Task<BonLivraison?> GetByNumeroAsync(string numero)
        => await db.BonsLivraison
            .Include(b => b.Client)
            .Include(b => b.CommandeVente)
            .Include(b => b.Lignes).ThenInclude(l => l.Produit)
            .FirstOrDefaultAsync(b => b.NumeroBonLivraison == numero);

    public async Task<BonLivraison> CreateAsync(BonLivraison bon, List<LigneBonLivraison> lignes)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "bons-livraison.create");
        LineCalculator.EnsureNoNegativeAmounts(lignes, l => l.Quantite, l => l.PrixUnitaire);

        bon.NumeroBonLivraison = await numService.NextBonLivraisonAsync();
        RecalculateTotals(bon, lignes);

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            db.BonsLivraison.Add(bon);
            foreach (var ligne in lignes)
            {
                ligne.NumeroBonLivraison = bon.NumeroBonLivraison;
                db.LignesBonLivraison.Add(ligne);
                await produitService.ApplyStockDeltaAsync(ligne.CodeProduit, -ligne.Quantite);
            }
            await db.SaveChangesGuardedAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        await journal.EnregistrerAsync("Ajout", "BonLivraison", bon.NumeroBonLivraison, bon.CodeClient);
        return bon;
    }

    public async Task UpdateAsync(BonLivraison bon, List<LigneBonLivraison> lignes)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "bons-livraison.update");
        LineCalculator.EnsureNoNegativeAmounts(lignes, l => l.Quantite, l => l.PrixUnitaire);

        var existing = await db.BonsLivraison
            .FirstOrDefaultAsync(b => b.NumeroBonLivraison == bon.NumeroBonLivraison)
            ?? throw new InvalidOperationException("Bon de livraison introuvable.");

        // Pas de AsNoTracking : voir DevisClientService.UpdateAsync pour le détail du risque de
        // conflit d'identité avec RemoveRange sur une entité déjà trackée dans le même scope.
        var oldLignes = await db.LignesBonLivraison
            .Where(l => l.NumeroBonLivraison == bon.NumeroBonLivraison)
            .ToListAsync();

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            foreach (var old in oldLignes)
                await produitService.ApplyStockDeltaAsync(old.CodeProduit, old.Quantite);
            db.LignesBonLivraison.RemoveRange(oldLignes);
            await db.SaveChangesGuardedAsync();

            existing.DateBonLivraison = bon.DateBonLivraison;
            existing.CodeClient = bon.CodeClient;
            existing.NumeroCommandeVente = bon.NumeroCommandeVente;
            existing.Remise = bon.Remise;
            existing.Note = bon.Note;
            existing.EtatBonLivraison = bon.EtatBonLivraison;
            existing.EtatFacture = bon.EtatFacture;

            var nouvellesLignes = lignes.Select(l => new LigneBonLivraison
            {
                NumeroBonLivraison = existing.NumeroBonLivraison,
                CodeProduit = l.CodeProduit,
                Quantite = l.Quantite,
                PrixUnitaire = l.PrixUnitaire,
                Remise = l.Remise,
                Tva = l.Tva,
                MontantHT = l.MontantHT
            }).ToList();

            RecalculateTotals(existing, nouvellesLignes);

            foreach (var ligne in nouvellesLignes)
            {
                db.LignesBonLivraison.Add(ligne);
                await produitService.ApplyStockDeltaAsync(ligne.CodeProduit, -ligne.Quantite);
            }
            await db.SaveChangesGuardedAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        await journal.EnregistrerAsync("Modification", "BonLivraison", existing.NumeroBonLivraison, existing.CodeClient);
    }

    public async Task DeleteAsync(string numero)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "bons-livraison.delete");

        // Pas de AsNoTracking : voir le commentaire dans UpdateAsync ci-dessus.
        var bon = await db.BonsLivraison
            .Include(b => b.Lignes)
            .FirstOrDefaultAsync(b => b.NumeroBonLivraison == numero);

        if (bon is null) return;

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            foreach (var ligne in bon.Lignes)
                await produitService.ApplyStockDeltaAsync(ligne.CodeProduit, ligne.Quantite);
            db.LignesBonLivraison.RemoveRange(bon.Lignes);
            db.BonsLivraison.Remove(bon);
            await db.SaveChangesGuardedAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        await journal.EnregistrerAsync("Suppression", "BonLivraison", numero);
    }

    public async Task<BonLivraison> CloneAsync(string numero)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "bons-livraison.create");

        var source = await GetByNumeroAsync(numero);
        if (source is null) throw new InvalidOperationException($"Bon de livraison {numero} introuvable.");

        var clone = new BonLivraison
        {
            NumeroBonLivraison = await numService.NextBonLivraisonAsync(),
            DateBonLivraison = DateTime.Today,
            CodeClient = source.CodeClient,
            NumeroCommandeVente = source.NumeroCommandeVente,
            Remise = source.Remise,
            Note = source.Note,
            EtatBonLivraison = "Ouvert",
            EtatFacture = "Non Facturé"
        };

        var lignesClone = source.Lignes.Select(l => new LigneBonLivraison
        {
            NumeroBonLivraison = clone.NumeroBonLivraison,
            CodeProduit = l.CodeProduit,
            Quantite = l.Quantite,
            PrixUnitaire = l.PrixUnitaire,
            Remise = l.Remise,
            Tva = l.Tva,
            MontantHT = l.MontantHT
        }).ToList();

        RecalculateTotals(clone, lignesClone);

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            db.BonsLivraison.Add(clone);
            db.LignesBonLivraison.AddRange(lignesClone);
            foreach (var l in lignesClone)
                await produitService.ApplyStockDeltaAsync(l.CodeProduit, -l.Quantite);
            await db.SaveChangesGuardedAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        await journal.EnregistrerAsync("Clone", "BonLivraison", clone.NumeroBonLivraison, $"cloné depuis {numero}");
        return clone;
    }

    public async Task<BonLivraison> CreateFromCommandeVenteAsync(string numeroCommandeVente)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "commandes-vente.update");

        var source = await db.CommandesVente
            .Include(c => c.Lignes)
            .FirstOrDefaultAsync(c => c.NumeroCommandeVente == numeroCommandeVente)
            ?? throw new InvalidOperationException($"Commande {numeroCommandeVente} introuvable.");

        var bon = new BonLivraison
        {
            DateBonLivraison = DateTime.Today,
            CodeClient = source.CodeClient,
            NumeroCommandeVente = source.NumeroCommandeVente,
            Remise = source.Remise,
            Note = source.Note,
            EtatBonLivraison = "Ouvert",
            EtatFacture = "Non Facturé"
        };

        var lignes = source.Lignes.Select(l => new LigneBonLivraison
        {
            CodeProduit = l.CodeProduit,
            Quantite = l.Quantite,
            PrixUnitaire = l.PrixUnitaire,
            Remise = l.Remise,
            Tva = l.Tva,
            MontantHT = l.MontantHT
        }).ToList();

        var created = await CreateAsync(bon, lignes);

        source.EtatLivraison = "Livré";
        await db.SaveChangesGuardedAsync();

        return created;
    }

    private static void RecalculateTotals(BonLivraison bon, List<LigneBonLivraison> lignes)
    {
        bon.MontantHT = lignes.Sum(l => l.MontantHT);
        bon.MontantTVA = lignes.Sum(l => l.Tva * l.MontantHT / 100);
        var remiseMontant = bon.MontantHT * bon.Remise / 100;
        bon.MontantTTC = bon.MontantHT - remiseMontant + bon.MontantTVA;
    }
}
