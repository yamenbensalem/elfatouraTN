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

public class CommandeVenteService(AppDbContext db, DocumentNumberService numService) : ICommandeVenteService
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
        commande.NumeroCommandeVente = await numService.NextCommandeVenteAsync();
        RecalculateTotals(commande, lignes);

        db.CommandesVente.Add(commande);
        foreach (var ligne in lignes)
        {
            ligne.NumeroCommandeVente = commande.NumeroCommandeVente;
            db.LignesCommandeVente.Add(ligne);
        }

        await db.SaveChangesAsync();
        return commande;
    }

    public async Task UpdateAsync(CommandeVente commande, List<LigneCommandeVente> lignes)
    {
        var oldLignes = await db.LignesCommandeVente
            .Where(l => l.NumeroCommandeVente == commande.NumeroCommandeVente)
            .ToListAsync();

        db.LignesCommandeVente.RemoveRange(oldLignes);
        RecalculateTotals(commande, lignes);

        db.CommandesVente.Update(commande);
        foreach (var ligne in lignes)
        {
            ligne.Id = 0;
            ligne.NumeroCommandeVente = commande.NumeroCommandeVente;
            db.LignesCommandeVente.Add(ligne);
        }

        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(string numero)
    {
        var commande = await db.CommandesVente
            .Include(c => c.Lignes)
            .FirstOrDefaultAsync(c => c.NumeroCommandeVente == numero);

        if (commande is null) return;

        db.LignesCommandeVente.RemoveRange(commande.Lignes);
        db.CommandesVente.Remove(commande);
        await db.SaveChangesAsync();
    }

    public async Task<CommandeVente> CloneAsync(string numero)
    {
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
        await db.SaveChangesAsync();
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
