using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;

namespace Web_T4C_GestCom.Services;

public interface IFactureClientService
{
    Task<List<FactureClient>> GetAllAsync(bool avoirsOnly = false, string? clientCode = null);
    Task<FactureClient?> GetByNumeroAsync(string numero);
    Task<FactureClient> CreateAsync(FactureClient facture, List<LigneFactureClient> lignes, AppConfigService config);
    Task UpdateAsync(FactureClient facture, List<LigneFactureClient> lignes);
    Task DeleteAsync(string numero);
    Task AddReglementAsync(ReglementFactureClient reglement);
    Task<double> GetSoldeAsync(string numero);
    Task<FactureClient> CloneAsync(string numero, bool isAvoir = false);
}

public class FactureClientService(
    AppDbContext db,
    DocumentNumberService numService,
    IProduitService produitService,
    IJournalActiviteService journal,
    ICurrentUserService? currentUser = null,
    IPermissionService? permissionService = null) : IFactureClientService
{
    public async Task<List<FactureClient>> GetAllAsync(bool avoirsOnly = false, string? clientCode = null)
    {
        var query = db.FacturesClient
            .AsNoTracking()
            .Include(f => f.Client)
            .Include(f => f.Reglements)
            .Where(f => f.IsAvoir == avoirsOnly)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(clientCode))
            query = query.Where(f => f.CodeClient == clientCode);

        return await query.OrderByDescending(f => f.DateFactureClient).ToListAsync();
    }

    public async Task<FactureClient?> GetByNumeroAsync(string numero)
        => await db.FacturesClient
            .AsNoTracking()
            .Include(f => f.Client)
            .Include(f => f.Lignes).ThenInclude(l => l.Produit)
            .Include(f => f.Reglements).ThenInclude(r => r.ModePayement)
            .FirstOrDefaultAsync(f => f.NumeroFactureClient == numero);

    public async Task<FactureClient> CreateAsync(FactureClient facture, List<LigneFactureClient> lignes, AppConfigService config)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "factures.create");
        LineCalculator.EnsureNoNegativeAmounts(lignes, l => l.Quantite, l => l.PrixUnitaire);

        facture.NumeroFactureClient = await numService.NextFactureClientAsync();
        facture.Timbre = config.TimbreFiscal;
        RecalculateTotals(facture, lignes, config);

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            db.FacturesClient.Add(facture);
            foreach (var ligne in lignes)
            {
                ligne.NumeroFactureClient = facture.NumeroFactureClient;
                db.LignesFactureClient.Add(ligne);
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

        await journal.EnregistrerAsync("Ajout", "FactureClient", facture.NumeroFactureClient, facture.CodeClient);
        return facture;
    }

    public async Task UpdateAsync(FactureClient facture, List<LigneFactureClient> lignes)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "factures.update");
        LineCalculator.EnsureNoNegativeAmounts(lignes, l => l.Quantite, l => l.PrixUnitaire);

        var existing = await db.FacturesClient
            .FirstOrDefaultAsync(f => f.NumeroFactureClient == facture.NumeroFactureClient)
            ?? throw new InvalidOperationException("Facture introuvable.");

        var oldLignes = await db.LignesFactureClient
            .Where(l => l.NumeroFactureClient == facture.NumeroFactureClient)
            .ToListAsync();

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            foreach (var old in oldLignes)
                await produitService.ApplyStockDeltaAsync(old.CodeProduit, old.Quantite);
            db.LignesFactureClient.RemoveRange(oldLignes);
            await db.SaveChangesGuardedAsync();

            existing.DateFactureClient = facture.DateFactureClient;
            existing.CodeClient = facture.CodeClient;
            existing.Remise = facture.Remise;
            existing.Timbre = facture.Timbre;
            existing.Note = facture.Note;
            existing.EtatFacture = facture.EtatFacture;
            existing.EtatReglement = facture.EtatReglement;
            existing.IsAvoir = facture.IsAvoir;
            existing.MontantHT = facture.MontantHT;
            existing.Fodec = facture.Fodec;
            existing.MontantTVA = facture.MontantTVA;
            existing.MontantTTC = facture.MontantTTC;

            var nouvellesLignes = lignes.Select(l => new LigneFactureClient
            {
                NumeroFactureClient = existing.NumeroFactureClient,
                CodeProduit = l.CodeProduit,
                Quantite = l.Quantite,
                PrixUnitaire = l.PrixUnitaire,
                Remise = l.Remise,
                Tva = l.Tva,
                Fodec = l.Fodec,
                MontantHT = l.MontantHT
            }).ToList();

            foreach (var ligne in nouvellesLignes)
            {
                db.LignesFactureClient.Add(ligne);
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

        await journal.EnregistrerAsync("Modification", "FactureClient", existing.NumeroFactureClient, existing.CodeClient);
    }

    public async Task DeleteAsync(string numero)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "factures.delete");

        // Pas de AsNoTracking : cette facture peut déjà être trackée dans le même scope (ex. un
        // règlement vient d'être ajouté) — un fetch détaché suivi de RemoveRange entrerait en
        // conflit d'identité avec l'entité déjà trackée. Risque réel en Blazor Server, où le scope
        // DbContext suit le circuit entier, pas juste cette requête.
        var facture = await db.FacturesClient
            .Include(f => f.Lignes)
            .Include(f => f.Reglements)
            .FirstOrDefaultAsync(f => f.NumeroFactureClient == numero);

        if (facture is null) return;

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            foreach (var ligne in facture.Lignes)
                await produitService.ApplyStockDeltaAsync(ligne.CodeProduit, ligne.Quantite);
            db.ReglementsFactureClient.RemoveRange(facture.Reglements);
            db.LignesFactureClient.RemoveRange(facture.Lignes);
            db.FacturesClient.Remove(facture);
            await db.SaveChangesGuardedAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        await journal.EnregistrerAsync("Suppression", "FactureClient", numero);
    }

    public async Task AddReglementAsync(ReglementFactureClient reglement)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "factures.update");

        db.ReglementsFactureClient.Add(reglement);
        await db.SaveChangesGuardedAsync();
        await UpdateEtatReglementAsync(reglement.NumeroFactureClient);
    }

    public async Task<double> GetSoldeAsync(string numero)
    {
        var facture = await db.FacturesClient
            .Include(f => f.Reglements)
            .FirstOrDefaultAsync(f => f.NumeroFactureClient == numero);

        if (facture is null) return 0;
        var totalRegle = facture.Reglements.Sum(r => r.Montant);
        return facture.MontantTTC + facture.Timbre - totalRegle;
    }

    public async Task<FactureClient> CloneAsync(string numero, bool isAvoir = false)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "factures.create");

        var source = await db.FacturesClient
            .Include(f => f.Lignes)
            .FirstOrDefaultAsync(f => f.NumeroFactureClient == numero);

        if (source is null) throw new InvalidOperationException($"Facture {numero} introuvable.");

        var clone = new FactureClient
        {
            NumeroFactureClient = await numService.NextFactureClientAsync(),
            DateFactureClient = DateTime.Today,
            CodeClient = source.CodeClient,
            Remise = source.Remise,
            Timbre = source.Timbre,
            Note = source.Note,
            EtatFacture = "Facture Ouverte",
            EtatReglement = "Non Réglé",
            IsAvoir = isAvoir
        };

        var lignesClone = source.Lignes.Select(l => new LigneFactureClient
        {
            NumeroFactureClient = clone.NumeroFactureClient,
            CodeProduit = l.CodeProduit,
            Quantite = l.Quantite,
            PrixUnitaire = l.PrixUnitaire,
            Remise = l.Remise,
            Tva = l.Tva,
            Fodec = l.Fodec,
            MontantHT = l.MontantHT
        }).ToList();

        clone.MontantHT = lignesClone.Sum(l => l.MontantHT);
        clone.Fodec = lignesClone.Sum(l => l.Fodec * l.MontantHT / 100);
        clone.MontantTVA = lignesClone.Sum(l => l.Tva * l.MontantHT / 100);
        var remiseMontant = clone.MontantHT * clone.Remise / 100;
        clone.MontantTTC = clone.MontantHT - remiseMontant + clone.Fodec + clone.MontantTVA;

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            db.FacturesClient.Add(clone);
            db.LignesFactureClient.AddRange(lignesClone);
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

        await journal.EnregistrerAsync("Clone", "FactureClient", clone.NumeroFactureClient, $"cloné depuis {numero}");
        return clone;
    }

    private static void RecalculateTotals(FactureClient facture, List<LigneFactureClient> lignes, AppConfigService config)
    {
        facture.MontantHT = lignes.Sum(l => l.MontantHT);
        facture.Fodec = lignes.Sum(l => l.Fodec * l.MontantHT / 100);
        facture.MontantTVA = lignes.Sum(l => l.Tva * l.MontantHT / 100);
        var remiseMontant = facture.MontantHT * facture.Remise / 100;
        facture.MontantTTC = facture.MontantHT - remiseMontant + facture.Fodec + facture.MontantTVA;
        facture.Timbre = config.TimbreFiscal;
    }

    private async Task UpdateEtatReglementAsync(string numero)
    {
        var facture = await db.FacturesClient
            .Include(f => f.Reglements)
            .FirstOrDefaultAsync(f => f.NumeroFactureClient == numero);

        if (facture is null) return;

        var totalRegle = facture.Reglements.Sum(r => r.Montant);
        var totalDu = facture.MontantTTC + facture.Timbre;

        facture.EtatReglement = totalRegle >= totalDu ? "Réglé" :
                                totalRegle > 0 ? "Partiellement Réglé" : "Non Réglé";

        await db.SaveChangesGuardedAsync();
    }
}
