using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Web_GestCom.Data;
using Web_GestCom.Data.Models;

namespace Web_GestCom.Services;

/// <summary>
/// CRUD for ApiKey — SuperAdmin-only platform-integration storage. Management page only: no
/// real REST API or third-party integration is wired to these records (see ApiKey doc comment).
/// </summary>
public interface IApiKeyService
{
    Task<List<ApiKey>> GetAllAsync();
    Task AddAsync(ApiKey apiKey);
    Task UpdateAsync(ApiKey apiKey);
    Task DeleteAsync(ApiKey apiKey);
}

public class ApiKeyService(AppDbContext db) : IApiKeyService
{
    public async Task<List<ApiKey>> GetAllAsync()
        => await db.ApiKeys.AsNoTracking()
            .Include(k => k.Company)
            .OrderByDescending(k => k.DateCreation)
            .ToListAsync();

    public async Task AddAsync(ApiKey apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey.Value))
            apiKey.Value = GenerateKeyValue();

        apiKey.DateCreation = DateTime.UtcNow;
        db.ApiKeys.Add(apiKey);
        await db.SaveChangesGuardedAsync();
    }

    public async Task UpdateAsync(ApiKey apiKey)
    {
        db.DetachStaleTrackedEntry(apiKey);
        db.ApiKeys.Update(apiKey);
        await db.SaveChangesGuardedAsync();
    }

    public async Task DeleteAsync(ApiKey apiKey)
    {
        db.DetachStaleTrackedEntry(apiKey);
        db.ApiKeys.Remove(apiKey);
        await db.SaveChangesGuardedAsync();
    }

    private static string GenerateKeyValue()
        => $"gc_{Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant()}";
}
