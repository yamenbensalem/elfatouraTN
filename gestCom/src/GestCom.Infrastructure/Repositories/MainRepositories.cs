using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using GestCom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestCom.Infrastructure.Repositories;

public class ClientRepository : Repository<Client>, IClientRepository
{
    public ClientRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Client?> GetByCodeAsync(string codeClient, string codeEntreprise)
    {
        return await _dbSet
            .Include(c => c.Devise)
            .FirstOrDefaultAsync(c => c.CodeClient == codeClient && c.CodeEntreprise == codeEntreprise);
    }

    public async Task<IEnumerable<Client>> GetClientsByEntrepriseAsync(string codeEntreprise)
    {
        return await _dbSet
            .Include(c => c.Devise)
            .Where(c => c.CodeEntreprise == codeEntreprise)
            .OrderBy(c => c.Nom)
            .ToListAsync();
    }

    public async Task<IEnumerable<Client>> SearchClientsAsync(string codeEntreprise, string searchTerm)
    {
        return await _dbSet
            .Where(c => c.CodeEntreprise == codeEntreprise &&
                       (c.CodeClient.Contains(searchTerm) ||
                        c.Nom.Contains(searchTerm) ||
                        c.MatriculeFiscale.Contains(searchTerm)))
            .OrderBy(c => c.Nom)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalCreancesAsync(string codeClient, string codeEntreprise)
    {
        var totalFactures = await _context.FacturesClient
            .Where(f => f.CodeClient == codeClient && 
                       f.CodeEntreprise == codeEntreprise &&
                       f.Statut != "Payée")
            .SumAsync(f => f.MontantRestant);

        return totalFactures;
    }
}

public class FournisseurRepository : Repository<Fournisseur>, IFournisseurRepository
{
    public FournisseurRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Fournisseur?> GetByCodeAsync(string codeFournisseur, string codeEntreprise)
    {
        return await _dbSet
            .Include(f => f.Devise)
            .FirstOrDefaultAsync(f => f.CodeFournisseur == codeFournisseur && f.CodeEntreprise == codeEntreprise);
    }

    public async Task<IEnumerable<Fournisseur>> GetFournisseursByEntrepriseAsync(string codeEntreprise)
    {
        return await _dbSet
            .Include(f => f.Devise)
            .Where(f => f.CodeEntreprise == codeEntreprise)
            .OrderBy(f => f.Nom)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalDettesAsync(string codeFournisseur, string codeEntreprise)
    {
        var totalFactures = await _context.FacturesFournisseur
            .Where(f => f.CodeFournisseur == codeFournisseur && 
                       f.CodeEntreprise == codeEntreprise &&
                       f.Statut != "Payée")
            .SumAsync(f => f.MontantRestant);

        return totalFactures;
    }

    public async Task<Fournisseur?> GetByMatriculeFiscaleAsync(string matriculeFiscale, string codeEntreprise)
    {
        return await _dbSet
            .FirstOrDefaultAsync(f => f.MatriculeFiscale == matriculeFiscale && f.CodeEntreprise == codeEntreprise);
    }
}

public class ProduitRepository : Repository<Produit>, IProduitRepository
{
    public ProduitRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Produit?> GetByCodeAsync(string codeProduit, string codeEntreprise)
    {
        return await _dbSet
            .Include(p => p.Devise)
            .Include(p => p.UniteProduit)
            .Include(p => p.TvaProduit)
            .Include(p => p.CategorieProduit)
            .Include(p => p.Fournisseur)
            .FirstOrDefaultAsync(p => p.CodeProduit == codeProduit && p.CodeEntreprise == codeEntreprise);
    }

    public async Task<IEnumerable<Produit>> GetProduitsByEntrepriseAsync(string codeEntreprise)
    {
        return await _dbSet
            .Include(p => p.CategorieProduit)
            .Include(p => p.UniteProduit)
            .Where(p => p.CodeEntreprise == codeEntreprise)
            .OrderBy(p => p.Designation)
            .ToListAsync();
    }

    public async Task<IEnumerable<Produit>> GetProduitsByCategorieAsync(int codeCategorie, string codeEntreprise)
    {
        return await _dbSet
            .Where(p => p.CodeCategorieProduit == codeCategorie && p.CodeEntreprise == codeEntreprise)
            .OrderBy(p => p.Designation)
            .ToListAsync();
    }

    public async Task<IEnumerable<Produit>> GetProduitsStockFaibleAsync(string codeEntreprise)
    {
        return await _dbSet
            .Where(p => p.CodeEntreprise == codeEntreprise && p.Quantite <= p.StockMinimal)
            .OrderBy(p => p.Quantite)
            .ToListAsync();
    }

    public async Task<IEnumerable<Produit>> SearchProduitsAsync(string codeEntreprise, string searchTerm)
    {
        return await _dbSet
            .Where(p => p.CodeEntreprise == codeEntreprise &&
                       (p.CodeProduit.Contains(searchTerm) ||
                        p.Designation.Contains(searchTerm)))
            .OrderBy(p => p.Designation)
            .ToListAsync();
    }

    public async Task<Produit?> GetByCodeBarreAsync(string codeBarre, string codeEntreprise)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.CodeBarre == codeBarre && p.CodeEntreprise == codeEntreprise);
    }

    public async Task<bool> HasLignesFactureAsync(string codeProduit, string codeEntreprise)
    {
        return await _context.LignesFactureClient
            .AnyAsync(l => l.CodeProduit == codeProduit);
    }

    public async Task<bool> HasLignesCommandeAsync(string codeProduit, string codeEntreprise)
    {
        return await _context.LignesCommandeVente
            .AnyAsync(l => l.CodeProduit == codeProduit);
    }

    public void Update(Produit produit)
    {
        _dbSet.Update(produit);
    }

    public void Delete(Produit produit)
    {
        _dbSet.Remove(produit);
    }
}
