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
}
