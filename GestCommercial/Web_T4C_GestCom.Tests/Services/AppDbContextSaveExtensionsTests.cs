using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;
using Web_T4C_GestCom.Tests.Helpers;
using Xunit;

namespace Web_T4C_GestCom.Tests.Services;

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
