namespace Web_GestCom.Services;

/// <summary>
/// Thrown instead of the raw EF Core <see cref="Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException"/>
/// when a save fails because the row was modified or deleted by someone else since it was loaded —
/// gives the UI (Web and Desktop alike) a message it can show directly, instead of EF's own
/// "The database operation was expected to affect 1 row(s), but actually affected 0 row(s)."
/// </summary>
public sealed class ConcurrencyConflictException : InvalidOperationException
{
    public ConcurrencyConflictException(Exception innerException)
        : base(
            "Cet enregistrement a été modifié ou supprimé par un autre utilisateur entre-temps. Rechargez la page et réessayez.",
            innerException)
    {
    }
}
