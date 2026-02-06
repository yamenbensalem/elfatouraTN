using GestCom.Application.Common.Interfaces;
using GestCom.Domain.Entities;
using GestCom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestCom.Infrastructure.Services;

/// <summary>
/// Service pour la gestion du stock
/// </summary>
public class StockService : IStockService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StockService> _logger;

    public StockService(ApplicationDbContext context, ILogger<StockService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Récupère le stock actuel d'un produit
    /// </summary>
    public async Task<decimal> GetStockActuelAsync(string codeProduit, string? codeMagasin = null)
    {
        var produit = await _context.Produits
            .FirstOrDefaultAsync(p => p.CodeProduit == codeProduit);

        return produit?.Quantite ?? 0;
    }

    /// <summary>
    /// Vérifie si le stock est suffisant
    /// </summary>
    public async Task<bool> VerifierStockSuffisantAsync(string codeProduit, decimal quantite, string? codeMagasin = null)
    {
        var stockActuel = await GetStockActuelAsync(codeProduit, codeMagasin);
        return stockActuel >= quantite;
    }

    /// <summary>
    /// Met à jour le stock après une sortie (vente/livraison)
    /// </summary>
    public async Task DecremenaterStockAsync(string codeProduit, decimal quantite, string? codeMagasin = null, string? reference = null)
    {
        var produit = await _context.Produits
            .FirstOrDefaultAsync(p => p.CodeProduit == codeProduit);

        if (produit == null)
        {
            _logger.LogWarning("Produit {CodeProduit} non trouvé pour décrémenter le stock", codeProduit);
            return;
        }

        produit.Quantite -= quantite;
        
        _logger.LogInformation(
            "Stock décrémenté pour {CodeProduit}: -{Quantite} (Ref: {Reference}). Nouveau stock: {StockActuel}",
            codeProduit, quantite, reference, produit.Quantite);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Met à jour le stock après une entrée (réception)
    /// </summary>
    public async Task IncremenaterStockAsync(string codeProduit, decimal quantite, string? codeMagasin = null, string? reference = null)
    {
        var produit = await _context.Produits
            .FirstOrDefaultAsync(p => p.CodeProduit == codeProduit);

        if (produit == null)
        {
            _logger.LogWarning("Produit {CodeProduit} non trouvé pour incrémenter le stock", codeProduit);
            return;
        }

        produit.Quantite += quantite;
        
        _logger.LogInformation(
            "Stock incrémenté pour {CodeProduit}: +{Quantite} (Ref: {Reference}). Nouveau stock: {StockActuel}",
            codeProduit, quantite, reference, produit.Quantite);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Calcule la valeur totale du stock
    /// </summary>
    public async Task<decimal> CalculerValeurStockAsync(string? codeCategorie = null)
    {
        var query = _context.Produits.AsQueryable();

        if (!string.IsNullOrEmpty(codeCategorie))
        {
            query = query.Where(p => p.CodeCategorie == codeCategorie);
        }

        return await query.SumAsync(p => p.Quantite * p.PrixAchatTTC);
    }
}
