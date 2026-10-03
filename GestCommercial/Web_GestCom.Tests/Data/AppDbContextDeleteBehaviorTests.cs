using Microsoft.EntityFrameworkCore;
using Web_GestCom.Data.Models;
using Web_GestCom.Tests.Helpers;
using Xunit;

namespace Web_GestCom.Tests.Data;

/// <summary>
/// EF Core's InMemory provider has no real foreign key engine — these tests assert on the model
/// metadata itself, which is what both EnsureCreated() (fresh installs) and TenantKeyMigration
/// (existing databases) are driven by. See AppDbContext.OnModelCreating. The traceability links
/// are ClientSetNull (cleared by the services before deleting), no longer SetNull: SQL Server
/// refuses ON DELETE SET NULL on a (CompanyId, numéro) link whose CompanyId is NOT NULL.
/// </summary>
public class AppDbContextDeleteBehaviorTests
{
    [Fact]
    public void BonLivraisonToCommandeVente_UsesClientSetNull_NotGlobalRestrict()
    {
        using var db = DbContextFactory.Create();

        var fk = db.Model.FindEntityType(typeof(BonLivraison))!
            .GetForeignKeys()
            .Single(f => f.PrincipalEntityType.ClrType == typeof(CommandeVente));

        Assert.Equal(DeleteBehavior.ClientSetNull, fk.DeleteBehavior);
    }

    [Fact]
    public void BonReceptionToCommandeAchat_UsesClientSetNull_NotGlobalRestrict()
    {
        using var db = DbContextFactory.Create();

        var fk = db.Model.FindEntityType(typeof(BonReception))!
            .GetForeignKeys()
            .Single(f => f.PrincipalEntityType.ClrType == typeof(CommandeAchat));

        Assert.Equal(DeleteBehavior.ClientSetNull, fk.DeleteBehavior);
    }

    [Fact]
    public void OtherDocumentForeignKeys_StillUseGlobalRestrict()
    {
        using var db = DbContextFactory.Create();

        // Spot-check a few FKs untouched by the SetNull exception: the global Restrict rule
        // (no orphaned/cascaded financial records) must still apply to everything else.
        var bonLivraisonToClient = db.Model.FindEntityType(typeof(BonLivraison))!
            .GetForeignKeys()
            .Single(f => f.PrincipalEntityType.ClrType == typeof(Client));
        var ligneFactureClientToFacture = db.Model.FindEntityType(typeof(LigneFactureClient))!
            .GetForeignKeys()
            .Single(f => f.PrincipalEntityType.ClrType == typeof(FactureClient));

        Assert.Equal(DeleteBehavior.Restrict, bonLivraisonToClient.DeleteBehavior);
        Assert.Equal(DeleteBehavior.Restrict, ligneFactureClientToFacture.DeleteBehavior);
    }
}
