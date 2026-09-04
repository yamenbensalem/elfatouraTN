using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;

namespace Web_T4C_GestCom.Services;

public interface ICommandeVenteService
{
    Task<List<CommandeVente>> GetAllAsync(string? clientCode = null);
    Task<CommandeVente?> GetByNumeroAsync(string numero);
    Task<CommandeVente> CreateAsync(CommandeVente commande, List<LigneCommandeVente> lignes);
    Task UpdateAsync(CommandeVente commande, List<LigneCommandeVente> lignes);
    Task DeleteAsync(string numero);
    Task<CommandeVente> CloneAsync(string numero);
}

public class CommandeVenteService(
    AppDbContext db,
    DocumentNumberService numService,
    ICurrentUserService? currentUser = null,
    IPermissionService? permissionService = null) : ICommandeVenteService
{
    public async Task<List<CommandeVente>> GetAllAsync(string? clientCode = null)
    {
        var query = db.CommandesVente
            .Include(c => c.Client)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(clientCode))
            query = query.Where(c => c.CodeClient == clientCode);

        return await query.OrderByDescending(c => c.DateCommandeVente).ToListAsync();
    }

    public async Task<CommandeVente?> GetByNumeroAsync(string numero)
        => await db.CommandesVente
            .Include(c => c.Client)
            .Include(c => c.Lignes).ThenInclude(l => l.Produit)
            .FirstOrDefaultAsync(c => c.NumeroCommandeVente == numero);

    public async Task<CommandeVente> CreateAsync(CommandeVente commande, List<LigneCommandeVente> lignes)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "commandes-vente.create");

        commande.NumeroCommandeVente = await numService.NextCommandeVenteAsync();
        RecalculateTotals(commande, lignes);

        db.CommandesVente.Add(commande);
        foreach (var ligne in lignes)
        {
            ligne.NumeroCommandeVente = commande.NumeroCommandeVente;
            db.LignesCommandeVente.Add(ligne);
        }

        await db.SaveChangesGuardedAsync();
        return commande;
    }

    public async Task UpdateAsync(CommandeVente commande, List<LigneCommandeVente> lignes)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "commandes-vente.update");

        var existing = await db.CommandesVente
            .FirstOrDefaultAsync(c => c.NumeroCommandeVente == commande.NumeroCommandeVente)
            ?? throw new InvalidOperationException("Commande introuvable.");

        // Pas de AsNoTracking : ces lignes peuvent déjà être trackées (ex. juste créées dans le même
        // scope) — voir DevisClientService.UpdateAsync pour le détail du risque de conflit d'identité.
        var oldLignes = await db.LignesCommandeVente
            .Where(l => l.NumeroCommandeVente == commande.NumeroCommandeVente)
            .ToListAsync();

        db.LignesCommandeVente.RemoveRange(oldLignes);
        await db.SaveChangesGuardedAsync();

        existing.DateCommandeVente = commande.DateCommandeVente;
        existing.CodeClient = commande.CodeClient;
        existing.Remise = commande.Remise;
        existing.EtatCommandeVente = commande.EtatCommandeVente;
        existing.EtatLivraison = commande.EtatLivraison;
        existing.Note = commande.Note;

        var nouvellesLignes = lignes.Select(l => new LigneCommandeVente
        {
            NumeroCommandeVente = existing.NumeroCommandeVente,
            CodeProduit = l.CodeProduit,
            Quantite = l.Quantite,
            PrixUnitaire = l.PrixUnitaire,
            Remise = l.Remise,
            Tva = l.Tva,
            MontantHT = l.MontantHT
        }).ToList();

        RecalculateTotals(existing, nouvellesLignes);

        db.LignesCommandeVente.AddRange(nouvellesLignes);

        await db.SaveChangesGuardedAsync();
    }

    public async Task DeleteAsync(string numero)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "commandes-vente.delete");

        // Pas de AsNoTracking : voir le commentaire dans UpdateAsync ci-dessus.
        var commande = await db.CommandesVente
            .Include(c => c.Lignes)
            .FirstOrDefaultAsync(c => c.NumeroCommandeVente == numero);

        if (commande is null) return;

        db.LignesCommandeVente.RemoveRange(commande.Lignes);
        db.CommandesVente.Remove(commande);
        await db.SaveChangesGuardedAsync();
    }

    public async Task<CommandeVente> CloneAsync(string numero)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "commandes-vente.create");

        var source = await GetByNumeroAsync(numero);
        if (source is null) throw new InvalidOperationException($"Commande {numero} introuvable.");

        var clone = new CommandeVente
        {
            NumeroCommandeVente = await numService.NextCommandeVenteAsync(),
            DateCommandeVente = DateTime.Today,
            CodeClient = source.CodeClient,
            Remise = source.Remise,
            Note = source.Note,
            EtatCommandeVente = "Ouvert",
            EtatLivraison = "Non Livré"
        };

        var lignesClone = source.Lignes.Select(l => new LigneCommandeVente
        {
            NumeroCommandeVente = clone.NumeroCommandeVente,
            CodeProduit = l.CodeProduit,
            Quantite = l.Quantite,
            PrixUnitaire = l.PrixUnitaire,
            Remise = l.Remise,
            Tva = l.Tva,
            MontantHT = l.MontantHT
        }).ToList();

        RecalculateTotals(clone, lignesClone);

        db.CommandesVente.Add(clone);
        db.LignesCommandeVente.AddRange(lignesClone);
        await db.SaveChangesGuardedAsync();
        return clone;
    }

    private static void RecalculateTotals(CommandeVente commande, List<LigneCommandeVente> lignes)
    {
        commande.MontantHT = lignes.Sum(l => l.MontantHT);
        commande.MontantTVA = lignes.Sum(l => l.Tva * l.MontantHT / 100);
        var remiseMontant = commande.MontantHT * commande.Remise / 100;
        commande.MontantTTC = commande.MontantHT - remiseMontant + commande.MontantTVA;
    }
}
