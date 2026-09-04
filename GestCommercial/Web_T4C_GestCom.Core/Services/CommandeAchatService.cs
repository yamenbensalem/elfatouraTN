using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;

namespace Web_T4C_GestCom.Services;

public interface ICommandeAchatService
{
    Task<List<CommandeAchat>> GetAllAsync();
    Task<CommandeAchat?> GetByNumeroAsync(string numero);
    Task<CommandeAchat> CreateAsync(CommandeAchat commande, List<LigneCommandeAchat> lignes);
    Task<CommandeAchat> UpdateAsync(CommandeAchat commande, List<LigneCommandeAchat> lignes);
    Task DeleteAsync(string numero);
    Task<CommandeAchat> CloneAsync(string numero);
}

public class CommandeAchatService(
    AppDbContext db,
    DocumentNumberService numService,
    ICurrentUserService? currentUser = null,
    IPermissionService? permissionService = null)
    : ICommandeAchatService
{
    public async Task<List<CommandeAchat>> GetAllAsync()
        => await db.CommandesAchat
            .Include(c => c.Fournisseur)
            .OrderByDescending(c => c.DateCommandeAchat)
            .ToListAsync();

    public async Task<CommandeAchat?> GetByNumeroAsync(string numero)
        => await db.CommandesAchat
            .Include(c => c.Fournisseur)
            .Include(c => c.Lignes).ThenInclude(l => l.Produit)
            .FirstOrDefaultAsync(c => c.NumeroCommandeAchat == numero);

    public async Task<CommandeAchat> CreateAsync(CommandeAchat commande, List<LigneCommandeAchat> lignes)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "commandes-achat.create");
        LineCalculator.EnsureNoNegativeAmounts(lignes, l => l.Quantite, l => l.PrixUnitaire);

        commande.NumeroCommandeAchat = await numService.NextCommandeAchatAsync();
        RecalculateTotals(commande, lignes);
        commande.Lignes = lignes;
        db.CommandesAchat.Add(commande);
        await db.SaveChangesGuardedAsync();
        return commande;
    }

    public async Task<CommandeAchat> UpdateAsync(CommandeAchat commande, List<LigneCommandeAchat> lignes)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "commandes-achat.update");
        LineCalculator.EnsureNoNegativeAmounts(lignes, l => l.Quantite, l => l.PrixUnitaire);

        var existing = await db.CommandesAchat
            .Include(c => c.Lignes)
            .FirstOrDefaultAsync(c => c.NumeroCommandeAchat == commande.NumeroCommandeAchat)
            ?? throw new InvalidOperationException("Commande introuvable.");

        db.LignesCommandeAchat.RemoveRange(existing.Lignes);
        await db.SaveChangesGuardedAsync();

        existing.DateCommandeAchat = commande.DateCommandeAchat;
        existing.CodeFournisseur = commande.CodeFournisseur;
        existing.EtatCommandeAchat = commande.EtatCommandeAchat;
        existing.EtatReception = commande.EtatReception;
        existing.Note = commande.Note;

        foreach (var l in lignes) l.Id = 0;
        RecalculateTotals(existing, lignes);
        existing.Lignes = lignes;
        await db.SaveChangesGuardedAsync();
        return existing;
    }

    public async Task DeleteAsync(string numero)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "commandes-achat.delete");

        // Pas de AsNoTracking : cette commande peut déjà être trackée dans le même scope — voir
        // DevisClientService.UpdateAsync pour le détail du risque de conflit d'identité avec RemoveRange.
        var commande = await db.CommandesAchat
            .Include(c => c.Lignes)
            .FirstOrDefaultAsync(c => c.NumeroCommandeAchat == numero)
            ?? throw new InvalidOperationException("Commande introuvable.");

        db.LignesCommandeAchat.RemoveRange(commande.Lignes);
        db.CommandesAchat.Remove(commande);
        await db.SaveChangesGuardedAsync();
    }

    public async Task<CommandeAchat> CloneAsync(string numero)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "commandes-achat.create");

        var source = await GetByNumeroAsync(numero)
            ?? throw new InvalidOperationException("Commande introuvable.");

        var clone = new CommandeAchat
        {
            NumeroCommandeAchat = await numService.NextCommandeAchatAsync(),
            DateCommandeAchat = DateTime.Today,
            CodeFournisseur = source.CodeFournisseur,
            EtatCommandeAchat = "Ouvert",
            EtatReception = "Non Reçu",
            Note = source.Note
        };

        var lignes = source.Lignes.Select(l => new LigneCommandeAchat
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
        db.CommandesAchat.Add(clone);
        await db.SaveChangesGuardedAsync();
        return clone;
    }

    private static void RecalculateTotals(CommandeAchat doc, List<LigneCommandeAchat> lignes)
    {
        doc.MontantHT = lignes.Sum(l => l.MontantHT);
        doc.MontantTVA = Math.Round(lignes.Sum(l => l.MontantHT * l.Tva / 100.0), 3);
        doc.MontantTTC = Math.Round(doc.MontantHT + doc.MontantTVA, 3);
    }
}
