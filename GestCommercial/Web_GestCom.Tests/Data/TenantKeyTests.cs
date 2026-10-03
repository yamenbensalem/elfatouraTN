using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Web_GestCom.Auth;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;
using Web_GestCom.Tests.Helpers;
using Xunit;

namespace Web_GestCom.Tests.Data;

/// <summary>
/// Clés "par entreprise" (CompanyId, code). Bug d'origine (reproduit en local le 2026-10-02) : le
/// code seul était la clé primaire, globale à la base, alors que la numérotation repart de CL00001
/// dans chaque entreprise — la deuxième entreprise ne pouvait créer aucun client.
/// </summary>
public class TenantKeyTests
{
    private static readonly Type[] TablesMetier =
    [
        typeof(Client), typeof(Fournisseur), typeof(Produit),
        typeof(DevisClient), typeof(CommandeVente), typeof(BonLivraison), typeof(FactureClient),
        typeof(CommandeAchat), typeof(BonReception), typeof(FactureFournisseur)
    ];

    private static DbContextOptions<AppDbContext> CreateOptions()
        => new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

    private static AppDbContext CreateTenantContext(DbContextOptions<AppDbContext> options, int companyId)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, "tenant-user"), new Claim("IsSuperAdmin", "0"), new Claim("CompanyId", companyId.ToString())],
            "Tests"));
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = principal } };
        var context = new AppDbContext(options, new HttpExecutionContext(accessor));
        context.Database.EnsureCreated();
        return context;
    }

    // ── Structure du modèle ───────────────────────────────────────────────

    [Fact]
    public void Model_BusinessTables_ShouldHaveCompanyPlusCodeAsTheirOnlyKey()
    {
        // Arrange
        using var db = DbContextFactory.Create();

        foreach (var type in TablesMetier)
        {
            // Act
            var entityType = db.Model.FindEntityType(type)!;
            var cle = entityType.FindPrimaryKey()!.Properties;

            // Assert — une clé secondaire sur le code seul recréerait l'unicité globale.
            Assert.Equal(2, cle.Count);
            Assert.Equal(nameof(ITenantOwned.CompanyId), cle[0].Name);
            Assert.Single(entityType.GetKeys());
            Assert.False(cle[0].IsNullable);
        }
    }

    [Fact]
    public void Model_EveryLinkToABusinessTable_ShouldCarryCompanyIdAndTheCode()
    {
        // Arrange
        using var db = DbContextFactory.Create();

        // Act
        var liens = db.Model.GetEntityTypes()
            .SelectMany(t => t.GetForeignKeys())
            .Where(fk => TablesMetier.Contains(fk.PrincipalEntityType.ClrType))
            .ToList();

        // Assert
        Assert.Equal(27, liens.Count);
        Assert.All(liens, fk =>
        {
            Assert.Equal(2, fk.Properties.Count);
            Assert.Equal(nameof(ITenantOwned.CompanyId), fk.Properties[0].Name);
            Assert.NotEqual(DeleteBehavior.SetNull, fk.DeleteBehavior);
            Assert.NotEqual(DeleteBehavior.Cascade, fk.DeleteBehavior);
        });
    }

    [Fact]
    public void Model_LinesAndPayments_ShouldBeTenantOwnedWithRequiredCompany()
    {
        // Arrange
        using var db = DbContextFactory.Create();

        foreach (var type in AppDbContext.TenantKeyedTypes)
        {
            // Act
            var companyId = db.Model.FindEntityType(type)!.FindProperty(nameof(ITenantOwned.CompanyId));

            // Assert
            Assert.NotNull(companyId);
            Assert.False(companyId.IsNullable);
            Assert.True(typeof(ITenantOwned).IsAssignableFrom(type));
        }
        Assert.Equal(19, AppDbContext.TenantKeyedTypes.Count);
    }

    // ── Deux entreprises, mêmes codes ─────────────────────────────────────

    [Fact]
    public async Task TwoCompanies_ShouldEachCreateClientProduitFournisseurWithTheSameCode()
    {
        // Arrange
        var options = CreateOptions();

        // Act
        foreach (var companyId in new[] { 1, 2 })
        {
            await using var context = CreateTenantContext(options, companyId);
            context.Clients.Add(new Client { CodeClient = "CL00001", NomClient = $"Client de l'entreprise {companyId}", CodeDevise = 1 });
            context.Fournisseurs.Add(new Fournisseur { CodeFournisseur = "FO00001", NomFournisseur = $"Fournisseur {companyId}" });
            context.Produits.Add(new Produit { CodeProduit = "PR00001", DesignationProduit = $"Produit {companyId}" });
            await context.SaveChangesAsync();
        }

        // Assert
        await using var verif = new AppDbContext(options);
        Assert.Equal(2, await verif.Clients.CountAsync(c => c.CodeClient == "CL00001"));
        Assert.Equal(2, await verif.Produits.CountAsync(p => p.CodeProduit == "PR00001"));
        await using var entreprise2 = CreateTenantContext(options, 2);
        var client = await entreprise2.Clients.SingleAsync();
        Assert.Equal("Client de l'entreprise 2", client.NomClient);
        Assert.Equal(2, client.CompanyId);
    }

    [Fact]
    public async Task TwoCompanies_ShouldEachGetTheirOwnFirstCodeFromTheGenerator()
    {
        // Arrange
        var options = CreateOptions();

        // Act
        var codes = new List<string>();
        foreach (var companyId in new[] { 1, 2 })
        {
            await using var context = CreateTenantContext(options, companyId);
            var code = await AppDbContextSaveExtensions.GenerateNextCodeAsync(context.Clients.Select(c => c.CodeClient), "CL", 5);
            context.Clients.Add(new Client { CodeClient = code, NomClient = "Premier client", CodeDevise = 1 });
            await context.SaveChangesAsync();
            codes.Add(code);
        }

        // Assert — numérotation continue propre à chaque entreprise, sans collision.
        Assert.Equal(["CL00001", "CL00001"], codes);
    }

    [Fact]
    public async Task TwoCompanies_ShouldEachCreateADocumentWithTheSameNumber_AndLinesFollowTheirCompany()
    {
        // Arrange
        var options = CreateOptions();

        // Act
        foreach (var companyId in new[] { 1, 2 })
        {
            await using var context = CreateTenantContext(options, companyId);
            context.Clients.Add(new Client { CodeClient = "CL00001", NomClient = "Client", CodeDevise = 1 });
            context.Produits.Add(new Produit { CodeProduit = "PR00001", DesignationProduit = "Produit" });
            context.DevisClient.Add(new DevisClient { NumeroDevis = "DV202610001", CodeClient = "CL00001", DateDevis = DateTime.Today });
            context.LignesDevisClient.Add(new LigneDevisClient { NumeroDevis = "DV202610001", CodeProduit = "PR00001", Quantite = companyId, PrixUnitaire = 10 });
            context.FacturesClient.Add(new FactureClient { NumeroFactureClient = "FC202610001", CodeClient = "CL00001", DateFactureClient = DateTime.Today });
            context.ReglementsFactureClient.Add(new ReglementFactureClient { NumeroFactureClient = "FC202610001", Montant = 5, CodeModePayement = 1 });
            await context.SaveChangesAsync();
        }

        // Assert
        await using var entreprise2 = CreateTenantContext(options, 2);
        var devis = await entreprise2.DevisClient.Include(d => d.Lignes).Include(d => d.Client).SingleAsync();
        var ligne = Assert.Single(devis.Lignes);
        Assert.Equal(2, ligne.CompanyId);
        Assert.Equal(2, ligne.Quantite);
        Assert.Equal(2, devis.Client!.CompanyId);
        Assert.Equal(1, await entreprise2.LignesDevisClient.CountAsync());
        Assert.Equal(2, (await entreprise2.ReglementsFactureClient.SingleAsync()).CompanyId);
    }

    [Fact]
    public async Task NewRow_WhenCallerSuppliesAnotherCompany_ShouldBeRejected()
    {
        // Arrange
        var options = CreateOptions();
        await using var context = CreateTenantContext(options, companyId: 1);
        context.Clients.Add(new Client { CodeClient = "CL00001", NomClient = "Intrus", CodeDevise = 1, CompanyId = 2 });

        // Act + Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task NewRow_WithoutExecutionContext_ShouldFallInTheDefaultCompany()
    {
        // Arrange — seed au démarrage et tests unitaires : pas de tenant courant.
        await using var db = DbContextFactory.Create();

        // Act
        var client = new Client { CodeClient = "CL00001", NomClient = "Seed", CodeDevise = 1 };
        db.Clients.Add(client);
        await db.SaveChangesAsync();

        // Assert
        Assert.Equal(Company.DefaultId, client.CompanyId);
    }

    // ── Liens de traçabilité : vidés par les services avant suppression ───

    [Fact]
    public async Task DeleteCommandeVente_WhenBonLivraisonLinked_ShouldClearTheLinkAndKeepTheBon()
    {
        // Arrange
        await using var db = DbContextFactory.Create();
        db.Clients.Add(new Client { CodeClient = "CL00001", NomClient = "Client", CodeDevise = 1 });
        db.CommandesVente.Add(new CommandeVente { NumeroCommandeVente = "CV202610001", CodeClient = "CL00001" });
        db.BonsLivraison.Add(new BonLivraison { NumeroBonLivraison = "BL202610001", CodeClient = "CL00001", NumeroCommandeVente = "CV202610001" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var svc = new CommandeVenteService(db, new DocumentNumberService(db), new NoOpJournalActiviteService());

        // Act
        await svc.DeleteAsync("CV202610001");

        // Assert
        Assert.Empty(db.CommandesVente);
        var bon = await db.BonsLivraison.AsNoTracking().SingleAsync();
        Assert.Null(bon.NumeroCommandeVente);
        Assert.Equal(Company.DefaultId, bon.CompanyId);
    }

    [Fact]
    public async Task DeleteCommandeAchat_WhenBonReceptionLinked_ShouldClearTheLinkAndKeepTheBon()
    {
        // Arrange
        await using var db = DbContextFactory.Create();
        db.Fournisseurs.Add(new Fournisseur { CodeFournisseur = "FO00001", NomFournisseur = "Fournisseur" });
        db.CommandesAchat.Add(new CommandeAchat { NumeroCommandeAchat = "CA202610001", CodeFournisseur = "FO00001" });
        db.BonsReception.Add(new BonReception { NumeroBonReception = "BR202610001", CodeFournisseur = "FO00001", NumeroCommandeAchat = "CA202610001" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var svc = new CommandeAchatService(db, new DocumentNumberService(db), new NoOpJournalActiviteService());

        // Act
        await svc.DeleteAsync("CA202610001");

        // Assert
        Assert.Empty(db.CommandesAchat);
        Assert.Null((await db.BonsReception.AsNoTracking().SingleAsync()).NumeroCommandeAchat);
    }
}
