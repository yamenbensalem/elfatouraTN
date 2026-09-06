using Microsoft.EntityFrameworkCore;
using Web_GestCom.Data;

namespace Web_GestCom.Services;

/// <summary>
/// Generates sequential document numbers in YYYYMM### format, mirroring
/// the desktop app's getNewCodeEntity() logic.
/// </summary>
public class DocumentNumberService(AppDbContext db)
{
    public async Task<string> NextDevisAsync()
        => await GenerateAsync("DV", db.DevisClient.Select(d => d.NumeroDevis));

    public async Task<string> NextCommandeVenteAsync()
        => await GenerateAsync("CV", db.CommandesVente.Select(c => c.NumeroCommandeVente));

    public async Task<string> NextBonLivraisonAsync()
        => await GenerateAsync("BL", db.BonsLivraison.Select(b => b.NumeroBonLivraison));

    public async Task<string> NextFactureClientAsync()
        => await GenerateAsync("FC", db.FacturesClient.Select(f => f.NumeroFactureClient));

    public async Task<string> NextCommandeAchatAsync()
        => await GenerateAsync("CA", db.CommandesAchat.Select(c => c.NumeroCommandeAchat));

    public async Task<string> NextBonReceptionAsync()
        => await GenerateAsync("BR", db.BonsReception.Select(b => b.NumeroBonReception));

    public async Task<string> NextFactureFournisseurAsync()
        => await GenerateAsync("FF", db.FacturesFournisseur.Select(f => f.NumeroFactureFournisseur));

    private static async Task<string> GenerateAsync(string prefix, IQueryable<string> existingKeys)
    {
        var yearMonth = DateTime.Today.ToString("yyyyMM");
        var pattern = $"{prefix}{yearMonth}";

        var last = await existingKeys
            .Where(k => k.StartsWith(pattern))
            .OrderByDescending(k => k)
            .FirstOrDefaultAsync();

        int next = 1;
        if (last is not null && int.TryParse(last[^3..], out var n))
            next = n + 1;

        return $"{pattern}{next:D3}";
    }
}
