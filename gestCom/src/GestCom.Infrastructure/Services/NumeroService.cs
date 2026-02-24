using GestCom.Application.Common.Interfaces;
using GestCom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestCom.Infrastructure.Services;

/// <summary>
/// Service pour la génération des numéros de documents
/// </summary>
public class NumeroService : INumeroService
{
    private readonly ApplicationDbContext _context;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<NumeroService> _logger;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public NumeroService(
        ApplicationDbContext context, 
        IDateTimeService dateTimeService,
        ILogger<NumeroService> logger)
    {
        _context = context;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    /// <summary>
    /// Génère le prochain numéro de facture client
    /// Format: FC{Année}-{Numéro:00000}
    /// </summary>
    public async Task<string> GenererNumeroFactureClientAsync(string codeEntreprise)
    {
        return await GenererNumeroAsync("FC", codeEntreprise, 
            async (prefixe, annee) => await GetDernierNumeroFactureClientAsync(prefixe, annee));
    }

    /// <summary>
    /// Génère le prochain numéro de devis
    /// Format: DV{Année}-{Numéro:00000}
    /// </summary>
    public async Task<string> GenererNumeroDevisAsync(string codeEntreprise)
    {
        return await GenererNumeroAsync("DV", codeEntreprise,
            async (prefixe, annee) => await GetDernierNumeroDevisAsync(prefixe, annee));
    }

    /// <summary>
    /// Génère le prochain numéro de bon de livraison
    /// Format: BL{Année}-{Numéro:00000}
    /// </summary>
    public async Task<string> GenererNumeroBonLivraisonAsync(string codeEntreprise)
    {
        return await GenererNumeroAsync("BL", codeEntreprise,
            async (prefixe, annee) => await GetDernierNumeroBonLivraisonAsync(prefixe, annee));
    }

    /// <summary>
    /// Génère le prochain numéro de commande vente
    /// Format: CV{Année}-{Numéro:00000}
    /// </summary>
    public async Task<string> GenererNumeroCommandeVenteAsync(string codeEntreprise)
    {
        return await GenererNumeroAsync("CV", codeEntreprise,
            async (prefixe, annee) => await GetDernierNumeroCommandeVenteAsync(prefixe, annee));
    }

    /// <summary>
    /// Génère le prochain numéro de facture fournisseur
    /// Format: FF{Année}-{Numéro:00000}
    /// </summary>
    public async Task<string> GenererNumeroFactureFournisseurAsync(string codeEntreprise)
    {
        return await GenererNumeroAsync("FF", codeEntreprise,
            async (prefixe, annee) => await GetDernierNumeroFactureFournisseurAsync(prefixe, annee));
    }

    /// <summary>
    /// Génère le prochain numéro de commande achat
    /// Format: CA{Année}-{Numéro:00000}
    /// </summary>
    public async Task<string> GenererNumeroCommandeAchatAsync(string codeEntreprise)
    {
        return await GenererNumeroAsync("CA", codeEntreprise,
            async (prefixe, annee) => await GetDernierNumeroCommandeAchatAsync(prefixe, annee));
    }

    /// <summary>
    /// Génère le prochain numéro de bon de réception
    /// Format: BR{Année}-{Numéro:00000}
    /// </summary>
    public async Task<string> GenererNumeroBonReceptionAsync(string codeEntreprise)
    {
        return await GenererNumeroAsync("BR", codeEntreprise,
            async (prefixe, annee) => await GetDernierNumeroBonReceptionAsync(prefixe, annee));
    }

    /// <summary>
    /// Génère le prochain numéro de règlement
    /// Format: RG{Année}-{Numéro:00000}
    /// </summary>
    public async Task<string> GenererNumeroReglementAsync(string codeEntreprise)
    {
        return await GenererNumeroAsync("RG", codeEntreprise,
            async (prefixe, annee) => await GetDernierNumeroReglementAsync(prefixe, annee));
    }

    private async Task<string> GenererNumeroAsync(string prefixe, string codeEntreprise, 
        Func<string, int, Task<int>> getDernierNumero)
    {
        await _semaphore.WaitAsync();
        try
        {
            var annee = _dateTimeService.Today.Year;
            var dernierNumero = await getDernierNumero(prefixe, annee);
            var nouveauNumero = dernierNumero + 1;
            
            var numero = $"{prefixe}{annee}-{nouveauNumero:D6}";
            
            _logger.LogDebug("Numéro généré: {Numero} pour entreprise {CodeEntreprise}", numero, codeEntreprise);
            
            return numero;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<int> GetDernierNumeroFactureClientAsync(string prefixe, int annee)
    {
        var pattern = $"{prefixe}{annee}-%";
        var dernierNumero = await _context.FacturesClient
            .Where(f => EF.Functions.Like(f.NumeroFacture, pattern))
            .OrderByDescending(f => f.NumeroFacture)
            .Select(f => f.NumeroFacture)
            .FirstOrDefaultAsync();

        return ExtraireNumero(dernierNumero);
    }

    private async Task<int> GetDernierNumeroDevisAsync(string prefixe, int annee)
    {
        var pattern = $"{prefixe}{annee}-%";
        var dernierNumero = await _context.DevisClients
            .Where(d => EF.Functions.Like(d.NumeroDevis, pattern))
            .OrderByDescending(d => d.NumeroDevis)
            .Select(d => d.NumeroDevis)
            .FirstOrDefaultAsync();

        return ExtraireNumero(dernierNumero);
    }

    private async Task<int> GetDernierNumeroBonLivraisonAsync(string prefixe, int annee)
    {
        var pattern = $"{prefixe}{annee}-%";
        var dernierNumero = await _context.BonsLivraison
            .Where(b => EF.Functions.Like(b.NumeroBon, pattern))
            .OrderByDescending(b => b.NumeroBon)
            .Select(b => b.NumeroBon)
            .FirstOrDefaultAsync();

        return ExtraireNumero(dernierNumero);
    }

    private async Task<int> GetDernierNumeroCommandeVenteAsync(string prefixe, int annee)
    {
        var pattern = $"{prefixe}{annee}-%";
        var dernierNumero = await _context.CommandesVente
            .Where(c => EF.Functions.Like(c.NumeroCommande, pattern))
            .OrderByDescending(c => c.NumeroCommande)
            .Select(c => c.NumeroCommande)
            .FirstOrDefaultAsync();

        return ExtraireNumero(dernierNumero);
    }

    private async Task<int> GetDernierNumeroFactureFournisseurAsync(string prefixe, int annee)
    {
        var pattern = $"{prefixe}{annee}-%";
        var dernierNumero = await _context.FacturesFournisseur
            .Where(f => EF.Functions.Like(f.NumeroFacture, pattern))
            .OrderByDescending(f => f.NumeroFacture)
            .Select(f => f.NumeroFacture)
            .FirstOrDefaultAsync();

        return ExtraireNumero(dernierNumero);
    }

    private async Task<int> GetDernierNumeroCommandeAchatAsync(string prefixe, int annee)
    {
        var pattern = $"{prefixe}{annee}-%";
        var dernierNumero = await _context.CommandesAchat
            .Where(c => EF.Functions.Like(c.NumeroCommande, pattern))
            .OrderByDescending(c => c.NumeroCommande)
            .Select(c => c.NumeroCommande)
            .FirstOrDefaultAsync();

        return ExtraireNumero(dernierNumero);
    }

    private async Task<int> GetDernierNumeroBonReceptionAsync(string prefixe, int annee)
    {
        var pattern = $"{prefixe}{annee}-%";
        var dernierNumero = await _context.BonsReception
            .Where(b => EF.Functions.Like(b.NumeroBon, pattern))
            .OrderByDescending(b => b.NumeroBon)
            .Select(b => b.NumeroBon)
            .FirstOrDefaultAsync();

        return ExtraireNumero(dernierNumero);
    }

    private async Task<int> GetDernierNumeroReglementAsync(string prefixe, int annee)
    {
        var pattern = $"{prefixe}{annee}-%";
        var dernierNumero = await _context.ReglementsFacture
            .Where(r => r.NumeroTransaction != null && EF.Functions.Like(r.NumeroTransaction, pattern))
            .OrderByDescending(r => r.NumeroTransaction)
            .Select(r => r.NumeroTransaction)
            .FirstOrDefaultAsync();

        return ExtraireNumero(dernierNumero);
    }

    private static int ExtraireNumero(string? numeroComplet)
    {
        if (string.IsNullOrEmpty(numeroComplet))
            return 0;

        var parties = numeroComplet.Split('-');
        if (parties.Length < 2)
            return 0;

        return int.TryParse(parties[^1], out var numero) ? numero : 0;
    }
}
