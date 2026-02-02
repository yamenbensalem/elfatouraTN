using Microsoft.EntityFrameworkCore;
using TunisianEInvoice.Application.Interfaces;
using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.Infrastructure.Persistence.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly EInvoiceDbContext _context;

        public ClientRepository(EInvoiceDbContext context)
        {
            _context = context;
        }

        public async Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<Client?> GetByMatriculeFiscalAsync(string matriculeFiscal, CancellationToken cancellationToken = default)
        {
            return await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.MatriculeFiscal == matriculeFiscal, cancellationToken);
        }

        public async Task<IEnumerable<Client>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Clients
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Client>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            var term = searchTerm.ToLower();
            return await _context.Clients
                .AsNoTracking()
                .Where(c => c.Name.ToLower().Contains(term) || 
                           c.MatriculeFiscal.ToLower().Contains(term) ||
                           (c.Email != null && c.Email.ToLower().Contains(term)))
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<Client> AddAsync(Client client, CancellationToken cancellationToken = default)
        {
            client.CreatedAt = DateTime.UtcNow;
            await _context.Clients.AddAsync(client, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return client;
        }

        public async Task UpdateAsync(Client client, CancellationToken cancellationToken = default)
        {
            client.UpdatedAt = DateTime.UtcNow;
            _context.Clients.Update(client);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var client = await _context.Clients.FindAsync(new object[] { id }, cancellationToken);
            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<bool> ExistsAsync(string matriculeFiscal, CancellationToken cancellationToken = default)
        {
            return await _context.Clients
                .AnyAsync(c => c.MatriculeFiscal == matriculeFiscal, cancellationToken);
        }
    }
}
