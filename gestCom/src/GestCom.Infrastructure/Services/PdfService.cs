using GestCom.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace GestCom.Infrastructure.Services;

/// <summary>
/// Service de génération de PDF (implémentation placeholder)
/// </summary>
public class PdfService : IPdfService
{
    private readonly ILogger<PdfService> _logger;

    public PdfService(ILogger<PdfService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> GenererFactureClientPdfAsync(string numeroFacture)
    {
        _logger.LogInformation("Génération PDF facture: {NumeroFacture}", numeroFacture);
        
        // TODO: Implémenter avec QuestPDF ou similaire
        // Pour l'instant, retourne un PDF vide
        return await Task.FromResult(Array.Empty<byte>());
    }

    public async Task<byte[]> GenererDevisPdfAsync(string numeroDevis)
    {
        _logger.LogInformation("Génération PDF devis: {NumeroDevis}", numeroDevis);
        return await Task.FromResult(Array.Empty<byte>());
    }

    public async Task<byte[]> GenererBonLivraisonPdfAsync(string numeroBL)
    {
        _logger.LogInformation("Génération PDF bon de livraison: {NumeroBL}", numeroBL);
        return await Task.FromResult(Array.Empty<byte>());
    }

    public async Task<byte[]> GenererCommandeAchatPdfAsync(string numeroCommande)
    {
        _logger.LogInformation("Génération PDF commande achat: {NumeroCommande}", numeroCommande);
        return await Task.FromResult(Array.Empty<byte>());
    }

    public async Task<byte[]> GenererBonCommandePdfAsync(string numeroCommande)
    {
        _logger.LogInformation("Génération PDF bon de commande: {NumeroCommande}", numeroCommande);
        return await Task.FromResult(Array.Empty<byte>());
    }

    public async Task<byte[]> GenererRapportPdfAsync(string typeRapport, object data)
    {
        _logger.LogInformation("Génération PDF rapport: {TypeRapport}", typeRapport);
        return await Task.FromResult(Array.Empty<byte>());
    }

    // Alias anglais pour la compatibilité avec les contrôleurs
    public Task<byte[]> GenerateFactureClientPdfAsync(string numeroFacture) 
        => GenererFactureClientPdfAsync(numeroFacture);

    public Task<byte[]> GenerateDevisClientPdfAsync(string numeroDevis) 
        => GenererDevisPdfAsync(numeroDevis);

    public Task<byte[]> GenerateBonLivraisonPdfAsync(string numeroBL) 
        => GenererBonLivraisonPdfAsync(numeroBL);

    public Task<byte[]> GenerateCommandeAchatPdfAsync(string numeroCommande) 
        => GenererCommandeAchatPdfAsync(numeroCommande);
}
