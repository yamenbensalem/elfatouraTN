using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Tests.Helpers;
using Xunit;

namespace Web_T4C_GestCom.Tests.Data;

/// <summary>
/// EF Core's InMemory provider has no real foreign key engine, so it can't reproduce what SQL
/// Server does with ON DELETE SET NULL — these tests instead assert on the model metadata itself,
/// which is what both EnsureCreated() (fresh installs) and the matching raw-SQL ALTER TABLE in
/// Program.cs (existing databases) are driven by. See AppDbContext.OnModelCreating.
/// </summary>
public class AppDbContextDeleteBehaviorTests
{
    [Fact]
    public void BonLivraisonToCommandeVente_UsesSetNull_NotGlobalRestrict()
    {
        using var db = DbContextFactory.Create();

        var fk = db.Model.FindEntityType(typeof(BonLivraison))!
            .GetForeignKeys()
            .Single(f => f.PrincipalEntityType.ClrType == typeof(CommandeVente));

        Assert.Equal(DeleteBehavior.SetNull, fk.DeleteBehavior);
    }

    [Fact]
    public void BonReceptionToCommandeAchat_UsesSetNull_NotGlobalRestrict()
    {
        using var db = DbContextFactory.Create();

        var fk = db.Model.FindEntityType(typeof(BonReception))!
            .GetForeignKeys()
            .Single(f => f.PrincipalEntityType.ClrType == typeof(CommandeAchat));

        Assert.Equal(DeleteBehavior.SetNull, fk.DeleteBehavior);
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
