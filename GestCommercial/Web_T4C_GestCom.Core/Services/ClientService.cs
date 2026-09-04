using Microsoft.EntityFrameworkCore;
using Web_T4C_GestCom.Data;
using Web_T4C_GestCom.Data.Models;

namespace Web_T4C_GestCom.Services;

public interface IClientService
{
    Task<List<Client>> GetAllAsync(string? search = null);
    Task<Client?> GetByCodeAsync(string code);
    Task<string> AddAsync(Client client);
    Task UpdateAsync(Client client);
    Task DeleteAsync(string code);
    Task<bool> ExistsAsync(string code);
}

public class ClientService(
    AppDbContext db,
    IJournalActiviteService journal,
    ICurrentUserService? currentUser = null,
    IPermissionService? permissionService = null) : IClientService
{
    public async Task<List<Client>> GetAllAsync(string? search = null)
    {
        var query = db.Clients.AsNoTracking().Include(c => c.Devise).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c =>
                c.NomClient.Contains(search) ||
                c.CodeClient.Contains(search) ||
                (c.MatriculeFiscale != null && c.MatriculeFiscale.Contains(search)) ||
                (c.Tel != null && c.Tel.Contains(search)));

        return await query.OrderBy(c => c.NomClient).ToListAsync();
    }

    public async Task<Client?> GetByCodeAsync(string code)
        => await db.Clients.AsNoTracking().Include(c => c.Devise).FirstOrDefaultAsync(c => c.CodeClient == code);

    public async Task<string> AddAsync(Client client)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "clients.create");

        // Auto-generate code if empty
        if (string.IsNullOrWhiteSpace(client.CodeClient))
        {
            var count = await db.Clients.CountAsync();
            client.CodeClient = $"CL{(count + 1):D5}";
        }
        db.Clients.Add(client);
        await db.SaveChangesGuardedAsync();
        await journal.EnregistrerAsync("Ajout", "Client", client.CodeClient, client.NomClient);
        return client.CodeClient;
    }

    public async Task UpdateAsync(Client client)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "clients.update");

        db.Clients.Update(client);
        await db.SaveChangesGuardedAsync();
        await journal.EnregistrerAsync("Modification", "Client", client.CodeClient, client.NomClient);
    }

    public async Task DeleteAsync(string code)
    {
        await ServicePermissionGuard.EnsureAsync(db, currentUser, permissionService, "clients.delete");

        var client = await db.Clients.FindAsync(code);
        if (client is not null)
        {
            db.Clients.Remove(client);
            await db.SaveChangesGuardedAsync();
            await journal.EnregistrerAsync("Suppression", "Client", code, client.NomClient);
        }
    }

    public async Task<bool> ExistsAsync(string code)
        => await db.Clients.AnyAsync(c => c.CodeClient == code);
}
