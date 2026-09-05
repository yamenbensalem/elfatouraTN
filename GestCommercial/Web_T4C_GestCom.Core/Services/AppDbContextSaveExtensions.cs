using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;

namespace Web_T4C_GestCom.Services;

/// <summary>
/// Drop-in replacement for <c>db.SaveChangesAsync()</c> across every service — translates
/// <see cref="DbUpdateConcurrencyException"/> into <see cref="ConcurrencyConflictException"/> once,
/// instead of every Add/Update/Delete method needing its own try/catch around the save.
/// </summary>
public static class AppDbContextSaveExtensions
{
    public static async Task<int> SaveChangesGuardedAsync(this AppDbContext db, CancellationToken cancellationToken = default)
    {
        try
        {
            return await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException(ex);
        }
    }

    /// <summary>
    /// Blazor Server keeps one scoped AppDbContext alive for the whole circuit, so an entity Added
    /// or Updated earlier in the session stays tracked. A later Update/Delete on the same row — via
    /// a freshly-loaded (AsNoTracking) instance with the same key — then fails with "another
    /// instance with the same key value is already being tracked". Call this before Update/Remove
    /// to detach that stale entry first so the new instance can be attached cleanly.
    /// </summary>
    public static void DetachStaleTrackedEntry<T>(this AppDbContext db, T entity) where T : class
    {
        var keyProperties = db.Model.FindEntityType(typeof(T))!.FindPrimaryKey()!.Properties;
        var staleEntry = db.ChangeTracker.Entries<T>().FirstOrDefault(e =>
            !ReferenceEquals(e.Entity, entity)
            && keyProperties.All(p => Equals(p.PropertyInfo!.GetValue(e.Entity), p.PropertyInfo!.GetValue(entity))));

        if (staleEntry is not null)
            staleEntry.State = EntityState.Detached;
    }
}
