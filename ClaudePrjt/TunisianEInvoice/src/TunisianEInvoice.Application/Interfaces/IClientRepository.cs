using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.Application.Interfaces
{
    public interface IClientRepository
    {
        Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Client?> GetByMatriculeFiscalAsync(string matriculeFiscal, CancellationToken cancellationToken = default);
        Task<IEnumerable<Client>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Client>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
        Task<Client> AddAsync(Client client, CancellationToken cancellationToken = default);
        Task UpdateAsync(Client client, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string matriculeFiscal, CancellationToken cancellationToken = default);
    }
}
