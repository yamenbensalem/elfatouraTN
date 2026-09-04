using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;

namespace Web_T4C_GestCom.Services;

public interface IDevisClientService
{
    Task<List<DevisClient>> GetAllAsync(string? clientCode = null);
    Task<DevisClient?> GetByNumeroAsync(string numero);
    Task<DevisClient> CreateAsync(DevisClient devis, List<LigneDevisClient> lignes, AppConfigService config);
    Task UpdateAsync(DevisClient devis, List<LigneDevisClient> lignes);
    Task DeleteAsync(string numero);
    Task<DevisClient> CloneAsync(string numero);
}

public class DevisClientService(
    AppDbContext db,
    DocumentNumberService numService,
    IJournalActiviteService journal,
    ICurrentUserService? currentUser = null,
    IPermissionService? permissionService = null) : IDevisClientService
{
    public async Task<List<DevisClient>> GetAllAsync(string? clientCode = null)
    {
        var query = db.DevisClient
            .Include(d => d.Client)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(clientCode))
            query = query.Where(d => d.CodeClient == clientCode);

        return await query.OrderByDescending(d => d.DateDevis).ToListAsync();
    }

    public async Task<DevisClient?> GetByNumeroAsync(string numero)
        => await db.DevisClient
            .Include(d => d.Client)
            .Include(d => d.Lignes).ThenInclude(l => l.Produit)
            .FirstOrDefaultAsync(d => d.NumeroDevis == numero);

    public async Task<DevisClient> CreateAsync(DevisClient devis, List<LigneDevisClient> lignes, AppConfigService config)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "devis.create");
        LineCalculator.EnsureNoNegativeAmounts(lignes, l => l.Quantite, l => l.PrixUnitaire);

        devis.NumeroDevis = await numService.NextDevisAsync();
        devis.Timbre = config.TimbreFiscal;
        RecalculateTotals(devis, lignes);

        db.DevisClient.Add(devis);
        foreach (var ligne in lignes)
        {
            ligne.NumeroDevis = devis.NumeroDevis;
            db.LignesDevisClient.Add(ligne);
        }

        await db.SaveChangesGuardedAsync();
        await journal.EnregistrerAsync("Ajout", "Devis", devis.NumeroDevis, devis.CodeClient);
        return devis;
    }

    public async Task UpdateAsync(DevisClient devis, List<LigneDevisClient> lignes)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "devis.update");
        LineCalculator.EnsureNoNegativeAmounts(lignes, l => l.Quantite, l => l.PrixUnitaire);

        var existing = await db.DevisClient
            .FirstOrDefaultAsync(d => d.NumeroDevis == devis.NumeroDevis)
            ?? throw new InvalidOperationException("Devis introuvable.");

        // Pas de AsNoTracking ici : ces lignes peuvent déjà être trackées (ex. juste créées dans le
        // même scope) — un fetch détaché suivi de RemoveRange entre en conflit avec l'entité déjà
        // trackée (EF lève une exception d'identité). Scope DbContext potentiellement long (Blazor
        // Server suit le circuit, pas juste une requête), donc ce risque est réel, pas théorique.
        var oldLignes = await db.LignesDevisClient
            .Where(l => l.NumeroDevis == devis.NumeroDevis)
            .ToListAsync();

        db.LignesDevisClient.RemoveRange(oldLignes);
        await db.SaveChangesGuardedAsync();

        existing.DateDevis = devis.DateDevis;
        existing.CodeClient = devis.CodeClient;
        existing.Remise = devis.Remise;
        existing.Timbre = devis.Timbre;
        existing.Note = devis.Note;
        existing.EtatDevis = devis.EtatDevis;

        var nouvellesLignes = lignes.Select(l => new LigneDevisClient
        {
            NumeroDevis = existing.NumeroDevis,
            CodeProduit = l.CodeProduit,
            Quantite = l.Quantite,
            PrixUnitaire = l.PrixUnitaire,
            Remise = l.Remise,
            Tva = l.Tva,
            MontantHT = l.MontantHT
        }).ToList();

        RecalculateTotals(existing, nouvellesLignes);

        db.LignesDevisClient.AddRange(nouvellesLignes);

        await db.SaveChangesGuardedAsync();
        await journal.EnregistrerAsync("Modification", "Devis", existing.NumeroDevis, existing.CodeClient);
    }

    public async Task DeleteAsync(string numero)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "devis.delete");

        // Pas de AsNoTracking : voir le commentaire dans UpdateAsync ci-dessus (risque de conflit
        // d'identité avec RemoveRange/Remove sur une entité déjà trackée dans le même scope).
        var devis = await db.DevisClient
            .Include(d => d.Lignes)
            .FirstOrDefaultAsync(d => d.NumeroDevis == numero);

        if (devis is null) return;

        db.LignesDevisClient.RemoveRange(devis.Lignes);
        db.DevisClient.Remove(devis);
        await db.SaveChangesGuardedAsync();
        await journal.EnregistrerAsync("Suppression", "Devis", numero);
    }

    public async Task<DevisClient> CloneAsync(string numero)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "devis.create");

        var source = await GetByNumeroAsync(numero);
        if (source is null) throw new InvalidOperationException($"Devis {numero} introuvable.");

        var clone = new DevisClient
        {
            NumeroDevis = await numService.NextDevisAsync(),
            DateDevis = DateTime.Today,
            CodeClient = source.CodeClient,
            Remise = source.Remise,
            Timbre = source.Timbre,
            Note = source.Note,
            EtatDevis = "Ouvert"
        };

        var lignesClone = source.Lignes.Select(l => new LigneDevisClient
        {
            NumeroDevis = clone.NumeroDevis,
            CodeProduit = l.CodeProduit,
            Quantite = l.Quantite,
            PrixUnitaire = l.PrixUnitaire,
            Remise = l.Remise,
            Tva = l.Tva,
            MontantHT = l.MontantHT
        }).ToList();

        RecalculateTotals(clone, lignesClone);

        db.DevisClient.Add(clone);
        db.LignesDevisClient.AddRange(lignesClone);
        await db.SaveChangesGuardedAsync();
        await journal.EnregistrerAsync("Clone", "Devis", clone.NumeroDevis, $"cloné depuis {numero}");
        return clone;
    }

    private static void RecalculateTotals(DevisClient devis, List<LigneDevisClient> lignes)
    {
        devis.MontantHT = lignes.Sum(l => l.MontantHT);
        devis.MontantTVA = lignes.Sum(l => l.Tva * l.MontantHT / 100);
        var remiseMontant = devis.MontantHT * devis.Remise / 100;
        devis.MontantTTC = devis.MontantHT - remiseMontant + devis.MontantTVA;
    }
}
