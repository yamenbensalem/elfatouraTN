namespace GestCom.Domain.Interfaces;

/// <summary>
/// Interface Unit of Work pour la gestion des transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Repositories Ventes
    IClientRepository Clients { get; }
    IDevisClientRepository DevisClients { get; }
    ICommandeVenteRepository CommandesVente { get; }
    IBonLivraisonRepository BonsLivraison { get; }
    IFactureClientRepository FacturesClient { get; }
    IReglementFactureRepository ReglementsFacture { get; }

    // Repositories Achats
    IFournisseurRepository Fournisseurs { get; }
    IDemandePrixRepository DemandesPrix { get; }
    ICommandeAchatRepository CommandesAchat { get; }
    IBonReceptionRepository BonsReception { get; }
    IFactureFournisseurRepository FacturesFournisseur { get; }
    IReglementFournisseurRepository ReglementsFournisseur { get; }

    // Repositories Stock
    IProduitRepository Produits { get; }
    ICategorieProduitRepository CategoriesProduit { get; }
    IMagasinProduitRepository MagasinsProduit { get; }

    // Repositories Configuration
    IEntrepriseRepository Entreprises { get; }
    IDeviseRepository Devises { get; }
    ITvaProduitRepository TVA { get; }
    IModePayementRepository ModesPayement { get; }
    IRetenuSourceRepository RetenuesSource { get; }
    IUniteProduitRepository Unites { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
