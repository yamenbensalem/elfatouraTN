using GestCom.Domain.Interfaces;
using GestCom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace GestCom.Infrastructure.Repositories;

/// <summary>
/// Implémentation de Unit of Work
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    // Repositories - Ventes
    private IClientRepository? _clients;
    private IDevisClientRepository? _devisClients;
    private ICommandeVenteRepository? _commandesVente;
    private IBonLivraisonRepository? _bonsLivraison;
    private IFactureClientRepository? _facturesClient;
    private IReglementFactureRepository? _reglementsFacture;

    // Repositories - Achats
    private IFournisseurRepository? _fournisseurs;
    private IDemandePrixRepository? _demandesPrix;
    private ICommandeAchatRepository? _commandesAchat;
    private IBonReceptionRepository? _bonsReception;
    private IFactureFournisseurRepository? _facturesFournisseur;
    private IReglementFournisseurRepository? _reglementsFournisseur;

    // Repositories - Stock
    private IProduitRepository? _produits;
    private ICategorieProduitRepository? _categoriesProduit;
    private IMagasinProduitRepository? _magasinsProduit;

    // Repositories - Configuration
    private IEntrepriseRepository? _entreprises;
    private IDeviseRepository? _devises;
    private ITvaProduitRepository? _tva;
    private IModePayementRepository? _modesPayement;
    private IRetenuSourceRepository? _retenuesSource;
    private IUniteProduitRepository? _unites;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    // Properties avec lazy loading
    public IClientRepository Clients => 
        _clients ??= new ClientRepository(_context);
        
    public IDevisClientRepository DevisClients => 
        _devisClients ??= new DevisClientRepository(_context);
        
    public ICommandeVenteRepository CommandesVente => 
        _commandesVente ??= new CommandeVenteRepository(_context);
        
    public IBonLivraisonRepository BonsLivraison => 
        _bonsLivraison ??= new BonLivraisonRepository(_context);
        
    public IFactureClientRepository FacturesClient => 
        _facturesClient ??= new FactureClientRepository(_context);
        
    public IReglementFactureRepository ReglementsFacture => 
        _reglementsFacture ??= new ReglementFactureRepository(_context);

    public IFournisseurRepository Fournisseurs => 
        _fournisseurs ??= new FournisseurRepository(_context);
        
    public IDemandePrixRepository DemandesPrix => 
        _demandesPrix ??= new DemandePrixRepository(_context);
        
    public ICommandeAchatRepository CommandesAchat => 
        _commandesAchat ??= new CommandeAchatRepository(_context);
        
    public IBonReceptionRepository BonsReception => 
        _bonsReception ??= new BonReceptionRepository(_context);
        
    public IFactureFournisseurRepository FacturesFournisseur => 
        _facturesFournisseur ??= new FactureFournisseurRepository(_context);
        
    public IReglementFournisseurRepository ReglementsFournisseur => 
        _reglementsFournisseur ??= new ReglementFournisseurRepository(_context);

    public IProduitRepository Produits => 
        _produits ??= new ProduitRepository(_context);
        
    public ICategorieProduitRepository CategoriesProduit => 
        _categoriesProduit ??= new CategorieProduitRepository(_context);
        
    public IMagasinProduitRepository MagasinsProduit => 
        _magasinsProduit ??= new MagasinProduitRepository(_context);

    public IEntrepriseRepository Entreprises => 
        _entreprises ??= new EntrepriseRepository(_context);
        
    public IDeviseRepository Devises => 
        _devises ??= new DeviseRepository(_context);
        
    public ITvaProduitRepository TVA => 
        _tva ??= new TvaProduitRepository(_context);
        
    public IModePayementRepository ModesPayement => 
        _modesPayement ??= new ModePayementRepository(_context);

    public IRetenuSourceRepository RetenuesSource => 
        _retenuesSource ??= new RetenuSourceRepository(_context);

    public IUniteProduitRepository Unites => 
        _unites ??= new UniteProduitRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
