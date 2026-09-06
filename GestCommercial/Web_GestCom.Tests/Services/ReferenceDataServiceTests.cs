using Microsoft.EntityFrameworkCore;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;
using Web_GestCom.Tests.Helpers;
using Xunit;

namespace Web_GestCom.Tests.Services;

public class ReferenceDataServiceTests
{
    private static ReferenceDataService<TvaProduit> CreateService(out AppDbContext db)
    {
        db = DbContextFactory.Create();
        return new ReferenceDataService<TvaProduit>(db);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllPersistedRows()
    {
        var svc = CreateService(out var db);
        var baseline = await db.TvasProduit.CountAsync();
        db.TvasProduit.Add(new TvaProduit { NomTvaProduit = "TVA custom A", TauxTvaProduit = 19 });
        db.TvasProduit.Add(new TvaProduit { NomTvaProduit = "TVA custom B", TauxTvaProduit = 7 });
        await db.SaveChangesAsync();

        var result = await svc.GetAllAsync();

        Assert.Equal(baseline + 2, result.Count);
    }

    [Fact]
    public async Task AddAsync_PersistsNewRow()
    {
        var svc = CreateService(out var db);
        var baseline = await db.TvasProduit.CountAsync();

        await svc.AddAsync(new TvaProduit { NomTvaProduit = "TVA custom", TauxTvaProduit = 13 });

        Assert.Equal(baseline + 1, await db.TvasProduit.CountAsync());
    }

    [Fact]
    public async Task UpdateAsync_PersistsChangedFields()
    {
        var svc = CreateService(out var db);
        var entity = new TvaProduit { NomTvaProduit = "TVA 19%", TauxTvaProduit = 19 };
        db.TvasProduit.Add(entity);
        await db.SaveChangesAsync();
        db.Entry(entity).State = EntityState.Detached;

        entity.NomTvaProduit = "TVA 19% (modifié)";
        entity.TauxTvaProduit = 20;
        await svc.UpdateAsync(entity);

        var reloaded = await db.TvasProduit.AsNoTracking().SingleAsync(t => t.CodeTvaProduit == entity.CodeTvaProduit);
        Assert.Equal("TVA 19% (modifié)", reloaded.NomTvaProduit);
        Assert.Equal(20, reloaded.TauxTvaProduit);
    }

    [Fact]
    public async Task DeleteAsync_RemovesRow()
    {
        var svc = CreateService(out var db);
        var baseline = await db.TvasProduit.CountAsync();
        var entity = new TvaProduit { NomTvaProduit = "TVA custom", TauxTvaProduit = 19 };
        db.TvasProduit.Add(entity);
        await db.SaveChangesAsync();
        db.Entry(entity).State = EntityState.Detached;

        await svc.DeleteAsync(entity);

        Assert.Equal(baseline, await db.TvasProduit.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_AfterEarlierAddOnSameContext_DoesNotThrowTrackingConflict()
    {
        // Blazor Server keeps one scoped AppDbContext alive for the whole circuit: an entity Added
        // earlier in the session stays tracked. A later Delete on that same row via a freshly-loaded
        // (AsNoTracking) instance — the shape every Parametres page actually uses — must not fail
        // with "another instance with the same key value is already being tracked".
        var svc = CreateService(out var db);
        var added = new TvaProduit { NomTvaProduit = "TVA custom", TauxTvaProduit = 19 };
        await svc.AddAsync(added); // leaves `added` tracked on `db`

        var loaded = await db.TvasProduit.AsNoTracking().SingleAsync(t => t.CodeTvaProduit == added.CodeTvaProduit);

        await svc.DeleteAsync(loaded);

        Assert.False(await db.TvasProduit.AnyAsync(t => t.CodeTvaProduit == added.CodeTvaProduit));
    }

    [Fact]
    public async Task UpdateAsync_AfterEarlierAddOnSameContext_DoesNotThrowTrackingConflict()
    {
        var svc = CreateService(out var db);
        var added = new TvaProduit { NomTvaProduit = "TVA custom", TauxTvaProduit = 19 };
        await svc.AddAsync(added); // leaves `added` tracked on `db`

        var loaded = await db.TvasProduit.AsNoTracking().SingleAsync(t => t.CodeTvaProduit == added.CodeTvaProduit);
        loaded.NomTvaProduit = "TVA custom (modifié)";

        await svc.UpdateAsync(loaded);

        var reloaded = await db.TvasProduit.AsNoTracking().SingleAsync(t => t.CodeTvaProduit == added.CodeTvaProduit);
        Assert.Equal("TVA custom (modifié)", reloaded.NomTvaProduit);
    }

    [Fact]
    public void TvaProduitToProduitForeignKey_UsesGlobalRestrict()
    {
        // EF Core's InMemory provider has no real foreign key engine, so it can't reproduce the
        // DbUpdateException SQL Server throws on a REFERENCE constraint violation (see
        // AppDbContextDeleteBehaviorTests) — assert on the model metadata instead: reference-data
        // rows still rely on AppDbContext's global DeleteBehavior.Restrict to block deletion while
        // in use, same as every other FK. DeleteErrorMessageHelper then turns that DbUpdateException
        // into the friendly message shown on each Parametres page.
        using var db = DbContextFactory.Create();

        var fk = db.Model.FindEntityType(typeof(Produit))!
            .GetForeignKeys()
            .Single(f => f.PrincipalEntityType.ClrType == typeof(TvaProduit));

        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }
}
