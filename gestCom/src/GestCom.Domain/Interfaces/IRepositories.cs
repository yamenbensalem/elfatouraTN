using GestCom.Domain.Entities;

namespace GestCom.Domain.Interfaces;

public interface IClientRepository : IRepository<Client>
{
    Task<Client?> GetByCodeAsync(string codeClient, string codeEntreprise);
    Task<IEnumerable<Client>> GetClientsByEntrepriseAsync(string codeEntreprise);
    Task<IEnumerable<Client>> SearchClientsAsync(string codeEntreprise, string searchTerm);
    Task<decimal> GetTotalCreancesAsync(string codeClient, string codeEntreprise);
}

public interface IFournisseurRepository : IRepository<Fournisseur>
{
    Task<Fournisseur?> GetByCodeAsync(string codeFournisseur, string codeEntreprise);
    Task<Fournisseur?> GetByMatriculeFiscaleAsync(string matriculeFiscale, string codeEntreprise);
    Task<IEnumerable<Fournisseur>> GetFournisseursByEntrepriseAsync(string codeEntreprise);
    Task<decimal> GetTotalDettesAsync(string codeFournisseur, string codeEntreprise);
}

public interface IProduitRepository : IRepository<Produit>
{
    Task<Produit?> GetByCodeAsync(string codeProduit, string codeEntreprise);
    Task<Produit?> GetByCodeBarreAsync(string codeBarre, string codeEntreprise);
    Task<IEnumerable<Produit>> GetProduitsByEntrepriseAsync(string codeEntreprise);
    Task<IEnumerable<Produit>> GetProduitsByCategorieAsync(int codeCategorie, string codeEntreprise);
    Task<IEnumerable<Produit>> GetProduitsStockFaibleAsync(string codeEntreprise);
    Task<IEnumerable<Produit>> SearchProduitsAsync(string codeEntreprise, string searchTerm);
    Task<bool> HasLignesFactureAsync(string codeProduit, string codeEntreprise);
    Task<bool> HasLignesCommandeAsync(string codeProduit, string codeEntreprise);
    void Update(Produit produit);
    void Delete(Produit produit);
}

public interface IDevisClientRepository : IRepository<DevisClient>
{
    Task<DevisClient?> GetByNumeroAsync(string numeroDevis, string codeEntreprise);
    Task<string?> GetLastNumeroAsync(string codeEntreprise);
    Task<IEnumerable<DevisClient>> GetDevisByClientAsync(string codeClient, string codeEntreprise);
    Task<IEnumerable<DevisClient>> GetDevisByStatutAsync(string statut, string codeEntreprise);
    void Update(DevisClient devis);
}

public interface ICommandeVenteRepository : IRepository<CommandeVente>
{
    Task<CommandeVente?> GetByNumeroAsync(string numeroCommande, string codeEntreprise);
    Task<string?> GetLastNumeroAsync(string codeEntreprise);
    Task<IEnumerable<CommandeVente>> GetCommandesByClientAsync(string codeClient, string codeEntreprise);
    Task<IEnumerable<CommandeVente>> GetCommandesByStatutAsync(string statut, string codeEntreprise);
    void Update(CommandeVente commande);
}

public interface IBonLivraisonRepository : IRepository<BonLivraison>
{
    Task<BonLivraison?> GetByNumeroAsync(string numeroBon, string codeEntreprise);
    Task<string?> GetLastNumeroAsync(string codeEntreprise);
    Task<IEnumerable<BonLivraison>> GetBonsByClientAsync(string codeClient, string codeEntreprise);
    Task<IEnumerable<BonLivraison>> GetBonsNonFacturesAsync(string codeEntreprise);
}

public interface IFactureClientRepository : IRepository<FactureClient>
{
    Task<FactureClient?> GetByNumeroAsync(string numeroFacture, string codeEntreprise);
    Task<string?> GetLastNumeroAsync(string codeEntreprise);
    Task<IEnumerable<FactureClient>> GetFacturesByClientAsync(string codeClient, string codeEntreprise);
    Task<IEnumerable<FactureClient>> GetFacturesImPayeesAsync(string codeEntreprise);
    Task<decimal> GetChiffreAffairesAsync(string codeEntreprise, DateTime dateDebut, DateTime dateFin);
    Task<Dictionary<string, decimal>> GetChiffreAffairesParMoisAsync(string codeEntreprise, DateTime dateDebut, DateTime dateFin);
}

public interface IReglementFactureRepository : IRepository<ReglementFacture>
{
    Task<IEnumerable<ReglementFacture>> GetReglementsByFactureAsync(string numeroFacture, string codeEntreprise);
    Task<IEnumerable<ReglementFacture>> GetReglementsByClientAsync(string codeClient, string codeEntreprise);
}

public interface IDemandePrixRepository : IRepository<DemandePrix>
{
    Task<DemandePrix?> GetByNumeroAsync(string numeroDemande, string codeEntreprise);
    Task<IEnumerable<DemandePrix>> GetDemandesByFournisseurAsync(string codeFournisseur, string codeEntreprise);
}

public interface ICommandeAchatRepository : IRepository<CommandeAchat>
{
    Task<CommandeAchat?> GetByNumeroAsync(string numeroCommande, string codeEntreprise);
    Task<IEnumerable<CommandeAchat>> GetCommandesByFournisseurAsync(string codeFournisseur, string codeEntreprise);
}

public interface IBonReceptionRepository : IRepository<BonReception>
{
    Task<BonReception?> GetByNumeroAsync(string numeroBon, string codeEntreprise);
    Task<IEnumerable<BonReception>> GetBonsByFournisseurAsync(string codeFournisseur, string codeEntreprise);
}

public interface IFactureFournisseurRepository : IRepository<FactureFournisseur>
{
    Task<FactureFournisseur?> GetByNumeroAsync(string numeroFacture, string codeEntreprise);
    Task<IEnumerable<FactureFournisseur>> GetFacturesByFournisseurAsync(string codeFournisseur, string codeEntreprise);
    Task<IEnumerable<FactureFournisseur>> GetFacturesImpayeesAsync(string codeEntreprise);
}

public interface IReglementFournisseurRepository : IRepository<ReglementFournisseur>
{
    Task<IEnumerable<ReglementFournisseur>> GetReglementsByFactureAsync(string numeroFacture, string codeEntreprise);
}

public interface ICategorieProduitRepository : IRepository<CategorieProduit>
{
    Task<CategorieProduit?> GetByCodeAsync(int codeCategorie, string codeEntreprise);
    Task<IEnumerable<CategorieProduit>> GetCategoriesPrincipalesAsync(string codeEntreprise);
    Task<IEnumerable<CategorieProduit>> GetSousCategoriesAsync(int codeCategorieParent, string codeEntreprise);
}

public interface IMagasinProduitRepository : IRepository<MagasinProduit>
{
    Task<IEnumerable<MagasinProduit>> GetMagasinsByEntrepriseAsync(string codeEntreprise);
}

public interface IEntrepriseRepository : IRepository<Entreprise>
{
    Task<Entreprise?> GetByCodeAsync(string codeEntreprise);
}

public interface IDeviseRepository : IRepository<Devise>
{
    Task<Devise?> GetDevisePrincipaleAsync();
}

public interface ITvaProduitRepository : IRepository<TvaProduit>
{
    Task<TvaProduit?> GetTVAParDefautAsync();
}

public interface IModePayementRepository : IRepository<ModePayement>
{
    Task<IEnumerable<ModePayement>> GetModesActifsAsync();
}

public interface IRetenuSourceRepository : IRepository<RetenuSource>
{
    Task<RetenuSource?> GetByCodeAsync(int codeRetenu);
    Task<IEnumerable<RetenuSource>> GetAllActiveAsync();
}

public interface IUniteProduitRepository : IRepository<UniteProduit>
{
    Task<IEnumerable<UniteProduit>> GetAllAsync();
}
