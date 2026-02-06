using GestCom.Domain.Entities;
using GestCom.Domain.Interfaces;
using GestCom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestCom.Infrastructure.Repositories;

// Repositories pour le module Ventes
public class DevisClientRepository : Repository<DevisClient>, IDevisClientRepository
{
    public DevisClientRepository(ApplicationDbContext context) : base(context) { }

    public async Task<DevisClient?> GetByNumeroAsync(string numeroDevis, string codeEntreprise)
    {
        return await _dbSet
            .Include(d => d.Client)
            .Include(d => d.Lignes)
            .ThenInclude(l => l.Produit)
            .FirstOrDefaultAsync(d => d.NumeroDevis == numeroDevis && d.CodeEntreprise == codeEntreprise);
    }

    public async Task<IEnumerable<DevisClient>> GetDevisByClientAsync(string codeClient, string codeEntreprise)
    {
        return await _dbSet
            .Where(d => d.CodeClient == codeClient && d.CodeEntreprise == codeEntreprise)
            .OrderByDescending(d => d.DateDevis)
            .ToListAsync();
    }

    public async Task<IEnumerable<DevisClient>> GetDevisByStatutAsync(string statut, string codeEntreprise)
    {
        return await _dbSet
            .Where(d => d.Statut == statut && d.CodeEntreprise == codeEntreprise)
            .OrderByDescending(d => d.DateDevis)
            .ToListAsync();
    }

    public async Task<string?> GetLastNumeroAsync(string codeEntreprise)
    {
        return await _dbSet
            .Where(d => d.CodeEntreprise == codeEntreprise)
            .OrderByDescending(d => d.NumeroDevis)
            .Select(d => d.NumeroDevis)
            .FirstOrDefaultAsync();
    }

    public void Update(DevisClient devis)
    {
        _dbSet.Update(devis);
    }
}

public class CommandeVenteRepository : Repository<CommandeVente>, ICommandeVenteRepository
{
    public CommandeVenteRepository(ApplicationDbContext context) : base(context) { }

    public async Task<CommandeVente?> GetByNumeroAsync(string numeroCommande, string codeEntreprise)
    {
        return await _dbSet
            .Include(c => c.Client)
            .Include(c => c.Lignes)
            .ThenInclude(l => l.Produit)
            .FirstOrDefaultAsync(c => c.NumeroCommande == numeroCommande && c.CodeEntreprise == codeEntreprise);
    }

    public async Task<IEnumerable<CommandeVente>> GetCommandesByClientAsync(string codeClient, string codeEntreprise)
    {
        return await _dbSet
            .Where(c => c.CodeClient == codeClient && c.CodeEntreprise == codeEntreprise)
            .OrderByDescending(c => c.DateCommande)
            .ToListAsync();
    }

    public async Task<IEnumerable<CommandeVente>> GetCommandesByStatutAsync(string statut, string codeEntreprise)
    {
        return await _dbSet
            .Where(c => c.Statut == statut && c.CodeEntreprise == codeEntreprise)
            .OrderByDescending(c => c.DateCommande)
            .ToListAsync();
    }

    public async Task<string?> GetLastNumeroAsync(string codeEntreprise)
    {
        return await _dbSet
            .Where(c => c.CodeEntreprise == codeEntreprise)
            .OrderByDescending(c => c.NumeroCommande)
            .Select(c => c.NumeroCommande)
            .FirstOrDefaultAsync();
    }

    public void Update(CommandeVente commande)
    {
        _dbSet.Update(commande);
    }
}

public class BonLivraisonRepository : Repository<BonLivraison>, IBonLivraisonRepository
{
    public BonLivraisonRepository(ApplicationDbContext context) : base(context) { }

    public async Task<BonLivraison?> GetByNumeroAsync(string numeroBon, string codeEntreprise)
    {
        return await _dbSet
            .Include(b => b.Client)
            .Include(b => b.Lignes)
            .ThenInclude(l => l.Produit)
            .FirstOrDefaultAsync(b => b.NumeroBon == numeroBon && b.CodeEntreprise == codeEntreprise);
    }

    public async Task<IEnumerable<BonLivraison>> GetBonsByClientAsync(string codeClient, string codeEntreprise)
    {
        return await _dbSet
            .Where(b => b.CodeClient == codeClient && b.CodeEntreprise == codeEntreprise)
            .OrderByDescending(b => b.DateLivraison)
            .ToListAsync();
    }

    public async Task<IEnumerable<BonLivraison>> GetBonsNonFacturesAsync(string codeEntreprise)
    {
        return await _dbSet
            .Where(b => !b.Facture && b.CodeEntreprise == codeEntreprise)
            .OrderByDescending(b => b.DateLivraison)
            .ToListAsync();
    }

    public async Task<string?> GetLastNumeroAsync(string codeEntreprise)
    {
        return await _dbSet
            .Where(b => b.CodeEntreprise == codeEntreprise)
            .OrderByDescending(b => b.NumeroBon)
            .Select(b => b.NumeroBon)
            .FirstOrDefaultAsync();
    }
}

public class FactureClientRepository : Repository<FactureClient>, IFactureClientRepository
{
    public FactureClientRepository(ApplicationDbContext context) : base(context) { }

    public async Task<FactureClient?> GetByNumeroAsync(string numeroFacture, string codeEntreprise)
    {
        return await _dbSet
            .Include(f => f.Client)
            .Include(f => f.Lignes)
            .ThenInclude(l => l.Produit)
            .Include(f => f.Reglements)
            .FirstOrDefaultAsync(f => f.NumeroFacture == numeroFacture && f.CodeEntreprise == codeEntreprise);
    }

    public async Task<IEnumerable<FactureClient>> GetFacturesByClientAsync(string codeClient, string codeEntreprise)
    {
        return await _dbSet
            .Where(f => f.CodeClient == codeClient && f.CodeEntreprise == codeEntreprise)
            .OrderByDescending(f => f.DateFacture)
            .ToListAsync();
    }

    public async Task<IEnumerable<FactureClient>> GetFacturesImpayeesAsync(string codeEntreprise)
    {
        return await _dbSet
            .Where(f => f.MontantRestant > 0 && f.CodeEntreprise == codeEntreprise)
            .OrderBy(f => f.DateEcheance)
            .ToListAsync();
    }

    public async Task<decimal> GetChiffreAffairesAsync(string codeEntreprise, DateTime dateDebut, DateTime dateFin)
    {
        return await _dbSet
            .Where(f => f.CodeEntreprise == codeEntreprise && 
                       f.DateFacture >= dateDebut && 
                       f.DateFacture <= dateFin &&
                       f.Statut == "Validée")
            .SumAsync(f => f.MontantTTC);
    }

    public async Task<string?> GetLastNumeroAsync(string codeEntreprise)
    {
        return await _dbSet
            .Where(f => f.CodeEntreprise == codeEntreprise)
            .OrderByDescending(f => f.NumeroFacture)
            .Select(f => f.NumeroFacture)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<FactureClient>> GetFacturesImPayeesAsync(string codeEntreprise)
    {
        return await _dbSet
            .Where(f => f.MontantRestant > 0 && f.CodeEntreprise == codeEntreprise)
            .OrderBy(f => f.DateEcheance)
            .ToListAsync();
    }

    public async Task<Dictionary<string, decimal>> GetChiffreAffairesParMoisAsync(string codeEntreprise, DateTime dateDebut, DateTime dateFin)
    {
        var factures = await _dbSet
            .Where(f => f.CodeEntreprise == codeEntreprise && 
                       f.DateFacture >= dateDebut && 
                       f.DateFacture <= dateFin &&
                       f.Statut == "Validée")
            .ToListAsync();

        return factures
            .GroupBy(f => f.DateFacture.ToString("yyyy-MM"))
            .ToDictionary(g => g.Key, g => g.Sum(f => f.MontantTTC));
    }
}

public class ReglementFactureRepository : Repository<ReglementFacture>, IReglementFactureRepository
{
    public ReglementFactureRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<ReglementFacture>> GetReglementsByFactureAsync(string numeroFacture, string codeEntreprise)
    {
        return await _dbSet
            .Where(r => r.NumeroFacture == numeroFacture && r.CodeEntreprise == codeEntreprise)
            .OrderByDescending(r => r.DateReglement)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReglementFacture>> GetReglementsByClientAsync(string codeClient, string codeEntreprise)
    {
        return await _dbSet
            .Where(r => r.CodeClient == codeClient && r.CodeEntreprise == codeEntreprise)
            .OrderByDescending(r => r.DateReglement)
            .ToListAsync();
    }
}

// Repositories similaires pour le module Achats
public class DemandePrixRepository : Repository<DemandePrix>, IDemandePrixRepository
{
    public DemandePrixRepository(ApplicationDbContext context) : base(context) { }

    public async Task<DemandePrix?> GetByNumeroAsync(string numeroDemande, string codeEntreprise)
    {
        return await _dbSet
            .Include(d => d.Fournisseur)
            .Include(d => d.Lignes)
            .ThenInclude(l => l.Produit)
            .FirstOrDefaultAsync(d => d.NumeroDemande == numeroDemande && d.CodeEntreprise == codeEntreprise);
    }

    public async Task<IEnumerable<DemandePrix>> GetDemandesByFournisseurAsync(string codeFournisseur, string codeEntreprise)
    {
        return await _dbSet
            .Where(d => d.CodeFournisseur == codeFournisseur && d.CodeEntreprise == codeEntreprise)
            .OrderByDescending(d => d.DateDemande)
            .ToListAsync();
    }
}

public class CommandeAchatRepository : Repository<CommandeAchat>, ICommandeAchatRepository
{
    public CommandeAchatRepository(ApplicationDbContext context) : base(context) { }

    public async Task<CommandeAchat?> GetByNumeroAsync(string numeroCommande, string codeEntreprise)
    {
        return await _dbSet
            .Include(c => c.Fournisseur)
            .Include(c => c.Lignes)
            .ThenInclude(l => l.Produit)
            .FirstOrDefaultAsync(c => c.NumeroCommande == numeroCommande && c.CodeEntreprise == codeEntreprise);
    }

    public async Task<IEnumerable<CommandeAchat>> GetCommandesByFournisseurAsync(string codeFournisseur, string codeEntreprise)
    {
        return await _dbSet
            .Where(c => c.CodeFournisseur == codeFournisseur && c.CodeEntreprise == codeEntreprise)
            .OrderByDescending(c => c.DateCommande)
            .ToListAsync();
    }
}

public class BonReceptionRepository : Repository<BonReception>, IBonReceptionRepository
{
    public BonReceptionRepository(ApplicationDbContext context) : base(context) { }

    public async Task<BonReception?> GetByNumeroAsync(string numeroBon, string codeEntreprise)
    {
        return await _dbSet
            .Include(b => b.Fournisseur)
            .Include(b => b.Lignes)
            .ThenInclude(l => l.Produit)
            .FirstOrDefaultAsync(b => b.NumeroBon == numeroBon && b.CodeEntreprise == codeEntreprise);
    }

    public async Task<IEnumerable<BonReception>> GetBonsByFournisseurAsync(string codeFournisseur, string codeEntreprise)
    {
        return await _dbSet
            .Where(b => b.CodeFournisseur == codeFournisseur && b.CodeEntreprise == codeEntreprise)
            .OrderByDescending(b => b.DateReception)
            .ToListAsync();
    }
}

public class FactureFournisseurRepository : Repository<FactureFournisseur>, IFactureFournisseurRepository
{
    public FactureFournisseurRepository(ApplicationDbContext context) : base(context) { }

    public async Task<FactureFournisseur?> GetByNumeroAsync(string numeroFacture, string codeEntreprise)
    {
        return await _dbSet
            .Include(f => f.Fournisseur)
            .Include(f => f.Lignes)
            .ThenInclude(l => l.Produit)
            .Include(f => f.Reglements)
            .FirstOrDefaultAsync(f => f.NumeroFacture == numeroFacture && f.CodeEntreprise == codeEntreprise);
    }

    public async Task<IEnumerable<FactureFournisseur>> GetFacturesByFournisseurAsync(string codeFournisseur, string codeEntreprise)
    {
        return await _dbSet
            .Where(f => f.CodeFournisseur == codeFournisseur && f.CodeEntreprise == codeEntreprise)
            .OrderByDescending(f => f.DateFacture)
            .ToListAsync();
    }

    public async Task<IEnumerable<FactureFournisseur>> GetFacturesImpayeesAsync(string codeEntreprise)
    {
        return await _dbSet
            .Where(f => f.MontantRestant > 0 && f.CodeEntreprise == codeEntreprise)
            .OrderBy(f => f.DateEcheance)
            .ToListAsync();
    }
}

public class ReglementFournisseurRepository : Repository<ReglementFournisseur>, IReglementFournisseurRepository
{
    public ReglementFournisseurRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<ReglementFournisseur>> GetReglementsByFactureAsync(string numeroFacture, string codeEntreprise)
    {
        return await _dbSet
            .Where(r => r.NumeroFacture == numeroFacture && r.CodeEntreprise == codeEntreprise)
            .OrderByDescending(r => r.DateReglement)
            .ToListAsync();
    }
}

// Repositories pour Stock et Configuration
public class CategorieProduitRepository : Repository<CategorieProduit>, ICategorieProduitRepository
{
    public CategorieProduitRepository(ApplicationDbContext context) : base(context) { }

    public async Task<CategorieProduit?> GetByCodeAsync(int codeCategorie, string codeEntreprise)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.CodeCategorie == codeCategorie && c.CodeEntreprise == codeEntreprise);
    }

    public async Task<IEnumerable<CategorieProduit>> GetCategoriesPrincipalesAsync(string codeEntreprise)
    {
        return await _dbSet
            .Where(c => c.CategorieParentId == null && c.CodeEntreprise == codeEntreprise)
            .ToListAsync();
    }

    public async Task<IEnumerable<CategorieProduit>> GetSousCategoriesAsync(int codeCategorieParent, string codeEntreprise)
    {
        return await _dbSet
            .Where(c => c.CategorieParentId == codeCategorieParent && c.CodeEntreprise == codeEntreprise)
            .ToListAsync();
    }
}

public class MagasinProduitRepository : Repository<MagasinProduit>, IMagasinProduitRepository
{
    public MagasinProduitRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<MagasinProduit>> GetMagasinsByEntrepriseAsync(string codeEntreprise)
    {
        return await _dbSet
            .Where(m => m.CodeEntreprise == codeEntreprise)
            .ToListAsync();
    }
}

public class EntrepriseRepository : Repository<Entreprise>, IEntrepriseRepository
{
    public EntrepriseRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Entreprise?> GetByCodeAsync(string codeEntreprise)
    {
        return await _dbSet
            .Include(e => e.Devise)
            .FirstOrDefaultAsync(e => e.CodeEntreprise == codeEntreprise);
    }
}

public class DeviseRepository : Repository<Devise>, IDeviseRepository
{
    public DeviseRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Devise?> GetDevisePrincipaleAsync()
    {
        return await _dbSet.FirstOrDefaultAsync(d => d.DevisePrincipale);
    }
}

public class TvaProduitRepository : Repository<TvaProduit>, ITvaProduitRepository
{
    public TvaProduitRepository(ApplicationDbContext context) : base(context) { }

    public async Task<TvaProduit?> GetTVAParDefautAsync()
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.ParDefaut);
    }
}

public class ModePayementRepository : Repository<ModePayement>, IModePayementRepository
{
    public ModePayementRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<ModePayement>> GetModesActifsAsync()
    {
        return await _dbSet.Where(m => m.Actif).ToListAsync();
    }
}

public class RetenuSourceRepository : Repository<RetenuSource>, IRetenuSourceRepository
{
    public RetenuSourceRepository(ApplicationDbContext context) : base(context) { }

    public async Task<RetenuSource?> GetByCodeAsync(int codeRetenu)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.CodeRetenu == codeRetenu);
    }

    public async Task<IEnumerable<RetenuSource>> GetAllActiveAsync()
    {
        return await _dbSet.ToListAsync();
    }
}

public class UniteProduitRepository : Repository<UniteProduit>, IUniteProduitRepository
{
    public UniteProduitRepository(ApplicationDbContext context) : base(context) { }

    public new async Task<IEnumerable<UniteProduit>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }
}
