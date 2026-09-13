using Microsoft.EntityFrameworkCore;
using Web_GestCom.Data.Models;
using Web_GestCom.Services;
using Web_GestCom.Tests.Helpers;
using Xunit;

namespace Web_GestCom.Tests.Services;

public class AppDbContextSaveExtensionsTests
{
    [Fact]
    public async Task SaveChangesGuardedAsync_RowDeletedByAnotherContext_ThrowsConcurrencyConflictException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var writerDb = DbContextFactory.Create(dbName);
        var editorDb = DbContextFactory.Create(dbName);

        writerDb.Clients.Add(new Client { CodeClient = "CL00001", NomClient = "Alpha SARL", CodeDevise = 1 });
        await writerDb.SaveChangesAsync();

        // "editor" loads the row to edit it, then someone else deletes that same row before editor saves.
        var loadedByEditor = await editorDb.Clients.FirstAsync(c => c.CodeClient == "CL00001");
        var loadedByWriter = await writerDb.Clients.FirstAsync(c => c.CodeClient == "CL00001");
        writerDb.Clients.Remove(loadedByWriter);
        await writerDb.SaveChangesAsync();

        loadedByEditor.NomClient = "Alpha SARL (modifié)";
        editorDb.Clients.Update(loadedByEditor);

        // Act
        var ex = await Assert.ThrowsAsync<ConcurrencyConflictException>(() => editorDb.SaveChangesGuardedAsync());

        // Assert
        Assert.IsType<DbUpdateConcurrencyException>(ex.InnerException);
        Assert.Contains("modifié ou supprimé par un autre utilisateur", ex.Message);
    }

    [Fact]
    public async Task SaveChangesGuardedAsync_RowConcurrentlyModifiedByAnotherContext_ThrowsConcurrencyConflictException()
    {
        // Real gap flagged by audit (2026-09-12) : the test above only covers a deleted row — a
        // still-EXISTING row modified concurrently by someone else ("dernier écrit gagne" silencieux)
        // went undetected before RowVersion ([Timestamp]) was added to the 7 document entities.
        var dbName = Guid.NewGuid().ToString();
        var writerDb = DbContextFactory.Create(dbName);
        var editorDb = DbContextFactory.Create(dbName);

        writerDb.FacturesClient.Add(new FactureClient
        {
            NumeroFactureClient = "FC202609001",
            DateFactureClient = DateTime.Today,
            CodeClient = "CL00001",
            EtatFacture = "Facture Ouverte"
        });
        await writerDb.SaveChangesAsync();

        // Two users open the same facture at (almost) the same time.
        var loadedByEditor = await editorDb.FacturesClient.FirstAsync(f => f.NumeroFactureClient == "FC202609001");
        var loadedByWriter = await writerDb.FacturesClient.FirstAsync(f => f.NumeroFactureClient == "FC202609001");

        // "writer" saves their change first. SQL Server bumps ROWVERSION automatically on every
        // UPDATE; the InMemory provider's value generator only fires on Add, not on Modified (its
        // "already has a value" check skips regeneration otherwise) — force a fresh value here to
        // faithfully simulate what a real ROWVERSION column does on this UPDATE.
        loadedByWriter.Note = "Modifié par un autre utilisateur";
        writerDb.Entry(loadedByWriter).Property(f => f.RowVersion).CurrentValue = Guid.NewGuid().ToByteArray();
        await writerDb.SaveChangesAsync();

        // "editor" still has the STALE RowVersion from their original load and tries to save their
        // own, different change over it.
        loadedByEditor.Note = "Mon changement (basé sur une version périmée)";

        var ex = await Assert.ThrowsAsync<ConcurrencyConflictException>(() => editorDb.SaveChangesGuardedAsync());

        Assert.IsType<DbUpdateConcurrencyException>(ex.InnerException);
        Assert.Contains("modifié ou supprimé par un autre utilisateur", ex.Message);
        // The row was NOT overwritten by the stale write — the writer's change survived.
        Assert.Equal("Modifié par un autre utilisateur", (await writerDb.FacturesClient.AsNoTracking().FirstAsync(f => f.NumeroFactureClient == "FC202609001")).Note);
    }

    [Fact]
    public async Task SaveChangesGuardedAsync_NoConflict_BehavesLikeSaveChangesAsync()
    {
        // Arrange
        var db = DbContextFactory.Create();
        db.Clients.Add(new Client { CodeClient = "CL00001", NomClient = "Alpha SARL", CodeDevise = 1 });

        // Act
        var affected = await db.SaveChangesGuardedAsync();

        // Assert
        Assert.Equal(1, affected);
        Assert.Equal(1, await db.Clients.CountAsync());
    }
}
