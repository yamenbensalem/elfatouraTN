namespace GestCom.Application.Common.Interfaces;

/// <summary>
/// Interface pour le service de génération de PDF
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Génère un PDF de facture client
    /// </summary>
    Task<byte[]> GenererFactureClientPdfAsync(string numeroFacture);
    
    /// <summary>
    /// Alias anglais pour GenererFactureClientPdfAsync
    /// </summary>
    Task<byte[]> GenerateFactureClientPdfAsync(string numeroFacture);

    /// <summary>
    /// Génère un PDF de devis
    /// </summary>
    Task<byte[]> GenererDevisPdfAsync(string numeroDevis);
    
    /// <summary>
    /// Alias anglais pour GenererDevisPdfAsync
    /// </summary>
    Task<byte[]> GenerateDevisClientPdfAsync(string numeroDevis);

    /// <summary>
    /// Génère un PDF de bon de livraison
    /// </summary>
    Task<byte[]> GenererBonLivraisonPdfAsync(string numeroBL);
    
    /// <summary>
    /// Alias anglais pour GenererBonLivraisonPdfAsync
    /// </summary>
    Task<byte[]> GenerateBonLivraisonPdfAsync(string numeroBL);

    /// <summary>
    /// Génère un PDF de commande achat
    /// </summary>
    Task<byte[]> GenererCommandeAchatPdfAsync(string numeroCommande);
    
    /// <summary>
    /// Alias anglais pour GenererCommandeAchatPdfAsync
    /// </summary>
    Task<byte[]> GenerateCommandeAchatPdfAsync(string numeroCommande);

    /// <summary>
    /// Génère un PDF de bon de commande
    /// </summary>
    Task<byte[]> GenererBonCommandePdfAsync(string numeroCommande);

    /// <summary>
    /// Génère un rapport en PDF
    /// </summary>
    Task<byte[]> GenererRapportPdfAsync(string typeRapport, object data);
}

/// <summary>
/// Interface pour le service de gestion des dates
/// </summary>
public interface IDateTimeService
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
    DateOnly Today { get; }
}

/// <summary>
/// Interface pour le service utilisateur courant
/// </summary>
public interface ICurrentUserService
{
    int? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    string? Role { get; }
    string? CodeEntreprise { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}

/// <summary>
/// Interface pour le service de gestion du stock
/// </summary>
public interface IStockService
{
    Task<decimal> GetStockActuelAsync(string codeProduit, string? codeMagasin = null);
    Task<bool> VerifierStockSuffisantAsync(string codeProduit, decimal quantite, string? codeMagasin = null);
    Task DecremenaterStockAsync(string codeProduit, decimal quantite, string? codeMagasin = null, string? reference = null);
    Task IncremenaterStockAsync(string codeProduit, decimal quantite, string? codeMagasin = null, string? reference = null);
    Task<decimal> CalculerValeurStockAsync(string? codeCategorie = null);
}

/// <summary>
/// Interface pour le service de numérotation
/// </summary>
public interface INumeroService
{
    Task<string> GenererNumeroFactureClientAsync(string codeEntreprise);
    Task<string> GenererNumeroDevisAsync(string codeEntreprise);
    Task<string> GenererNumeroBonLivraisonAsync(string codeEntreprise);
    Task<string> GenererNumeroCommandeVenteAsync(string codeEntreprise);
    Task<string> GenererNumeroFactureFournisseurAsync(string codeEntreprise);
    Task<string> GenererNumeroCommandeAchatAsync(string codeEntreprise);
    Task<string> GenererNumeroBonReceptionAsync(string codeEntreprise);
    Task<string> GenererNumeroReglementAsync(string codeEntreprise);
}
