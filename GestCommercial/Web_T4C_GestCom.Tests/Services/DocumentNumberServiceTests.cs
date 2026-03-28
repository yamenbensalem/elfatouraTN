using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;
using Web_T4C_GestCom.Tests.Helpers;
using Xunit;

namespace Web_T4C_GestCom.Tests.Services;

public class DocumentNumberServiceTests
{
    // Mirrors the runtime value used inside DocumentNumberService.GenerateAsync()
    private static string YM => DateTime.Today.ToString("yyyyMM");

    private static (DocumentNumberService svc, AppDbContext db) Create()
    {
        var db = DbContextFactory.Create();
        return (new DocumentNumberService(db), db);
    }

    // ── FactureClient ────────────────────────────────────────────────────────

    [Fact]
    public async Task NextFactureClientAsync_EmptyTable_Returns001()
    {
        var (svc, _) = Create();
        Assert.Equal($"FC{YM}001", await svc.NextFactureClientAsync());
    }

    [Fact]
    public async Task NextFactureClientAsync_OneExisting_Returns002()
    {
        var (svc, db) = Create();
        db.Clients.Add(new Client { CodeClient = "CL00001", NomClient = "Test", CodeDevise = 1 });
        db.FacturesClient.Add(new FactureClient
        {
            NumeroFactureClient = $"FC{YM}001",
            CodeClient = "CL00001",
            DateFactureClient = DateTime.Today
        });
        await db.SaveChangesAsync();

        Assert.Equal($"FC{YM}002", await svc.NextFactureClientAsync());
    }

    [Fact]
    public async Task NextFactureClientAsync_FiveExisting_Returns006()
    {
        var (svc, db) = Create();
        db.Clients.Add(new Client { CodeClient = "CL00001", NomClient = "Test", CodeDevise = 1 });
        for (int i = 1; i <= 5; i++)
            db.FacturesClient.Add(new FactureClient
            {
                NumeroFactureClient = $"FC{YM}{i:D3}",
                CodeClient = "CL00001",
                DateFactureClient = DateTime.Today
            });
        await db.SaveChangesAsync();

        Assert.Equal($"FC{YM}006", await svc.NextFactureClientAsync());
    }

    // ── Devis ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task NextDevisAsync_EmptyTable_Returns001()
    {
        var (svc, _) = Create();
        Assert.Equal($"DV{YM}001", await svc.NextDevisAsync());
    }

    [Fact]
    public async Task NextDevisAsync_OneExisting_Returns002()
    {
        var (svc, db) = Create();
        db.Clients.Add(new Client { CodeClient = "CL00001", NomClient = "Test", CodeDevise = 1 });
        db.DevisClient.Add(new DevisClient
        {
            NumeroDevis = $"DV{YM}001",
            CodeClient = "CL00001",
            DateDevis = DateTime.Today
        });
        await db.SaveChangesAsync();

        Assert.Equal($"DV{YM}002", await svc.NextDevisAsync());
    }

    // ── CommandeVente ─────────────────────────────────────────────────────────

    [Fact]
    public async Task NextCommandeVenteAsync_EmptyTable_Returns001()
    {
        var (svc, _) = Create();
        Assert.Equal($"CV{YM}001", await svc.NextCommandeVenteAsync());
    }

    // ── BonLivraison ──────────────────────────────────────────────────────────

    [Fact]
    public async Task NextBonLivraisonAsync_EmptyTable_Returns001()
    {
        var (svc, _) = Create();
        Assert.Equal($"BL{YM}001", await svc.NextBonLivraisonAsync());
    }

    // ── CommandeAchat ─────────────────────────────────────────────────────────

    [Fact]
    public async Task NextCommandeAchatAsync_EmptyTable_Returns001()
    {
        var (svc, _) = Create();
        Assert.Equal($"CA{YM}001", await svc.NextCommandeAchatAsync());
    }

    // ── BonReception ──────────────────────────────────────────────────────────

    [Fact]
    public async Task NextBonReceptionAsync_EmptyTable_Returns001()
    {
        var (svc, _) = Create();
        Assert.Equal($"BR{YM}001", await svc.NextBonReceptionAsync());
    }

    // ── FactureFournisseur ────────────────────────────────────────────────────

    [Fact]
    public async Task NextFactureFournisseurAsync_EmptyTable_Returns001()
    {
        var (svc, _) = Create();
        Assert.Equal($"FF{YM}001", await svc.NextFactureFournisseurAsync());
    }

    // ── Prefix isolation ─────────────────────────────────────────────────────

    [Fact]
    public async Task DifferentPrefixes_DoNotInterfereBetweenTables()
    {
        var (svc, db) = Create();
        // Seed one facture client
        db.Clients.Add(new Client { CodeClient = "CL00001", NomClient = "Test", CodeDevise = 1 });
        db.FacturesClient.Add(new FactureClient
        {
            NumeroFactureClient = $"FC{YM}001",
            CodeClient = "CL00001",
            DateFactureClient = DateTime.Today
        });
        await db.SaveChangesAsync();

        // Devis counter is independent of FactureClient counter
        Assert.Equal($"DV{YM}001", await svc.NextDevisAsync());
        Assert.Equal($"FC{YM}002", await svc.NextFactureClientAsync());
    }
}
