using Web_T4C_GestCom.Data.Models;
using Web_T4C_GestCom.Services;

namespace Web_T4C_GestCom.Tests.Helpers;

/// <summary>
/// No-op implementation of <see cref="IJournalActiviteService"/> for unit tests.
/// All journal writes are silently discarded so tests remain fast and focused.
/// </summary>
public sealed class NoOpJournalActiviteService : IJournalActiviteService
{
    public Task EnregistrerAsync(string action, string entite, string? codeEntite = null, string? detail = null)
        => Task.CompletedTask;

    public Task<List<JournalActivite>> GetAllAsync(
        string? login = null,
        string? entite = null,
        DateTime? debut = null,
        DateTime? fin = null)
        => Task.FromResult(new List<JournalActivite>());

    public Task<List<string>> GetLoginsDistinctsAsync()
        => Task.FromResult(new List<string>());

    public Task<List<string>> GetEntitesDistinctesAsync()
        => Task.FromResult(new List<string>());

    public Task<int> PurgeAsync(int olderThanMonths)
        => Task.FromResult(0);
}
