using Microsoft.EntityFrameworkCore;
using Web_GestCom.Data;

namespace Web_GestCom.Services;

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
        catch (DbUpdateException ex) when (ex.InnerException is not null)
        {
            // EF Core's default DbUpdateException.Message is the unhelpful generic "An error
            // occurred while saving the entity changes. See the inner exception for details." —
            // the real SQL error (constraint violation, truncation, etc.) is only ever in
            // InnerException.Message, which the UI's generic `catch (Exception ex) { ex.Message }`
            // blocks never see. Surface it here once instead of duplicating this in every page.
            throw new DbUpdateException($"{ex.Message} {ex.InnerException.Message}", ex);
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

    /// <summary>
    /// Computes the next "PREFIX#####"-style code from the MAX existing number under that prefix —
    /// never from COUNT(*). COUNT(*) breaks the moment any row is deleted: a lower count can
    /// recompute a number that still exists on a surviving row, causing a primary-key violation on
    /// insert (real bug hit in production: "PR00001" re-generated after PR00002/.../PR0000N had
    /// been deleted but PR00001 itself was still there). Mirrors DocumentNumberService's approach
    /// for document numbers. Codes that don't parse as "prefix + digits" (e.g. a custom code typed
    /// by the user) are simply ignored rather than breaking generation.
    /// </summary>
    public static async Task<string> GenerateNextCodeAsync(IQueryable<string> existingCodes, string prefix, int numberLength)
    {
        var codes = await existingCodes.Where(c => c.StartsWith(prefix)).ToListAsync();
        var maxNumber = codes
            .Select(c => int.TryParse(c.AsSpan(prefix.Length), out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();
        return $"{prefix}{(maxNumber + 1).ToString($"D{numberLength}")}";
    }
}
