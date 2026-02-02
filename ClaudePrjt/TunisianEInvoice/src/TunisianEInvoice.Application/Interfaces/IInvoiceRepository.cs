using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.Application.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<InvoiceRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<InvoiceRecord?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<InvoiceRecord>> GetByClientAsync(Guid clientId, CancellationToken cancellationToken = default);
        Task<IEnumerable<InvoiceRecord>> GetByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<InvoiceRecord>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
        Task<InvoiceRecord?> GetByTtnReferenceAsync(string ttnReference, CancellationToken cancellationToken = default);
        Task<InvoiceRecord> AddAsync(InvoiceRecord invoice, CancellationToken cancellationToken = default);
        Task UpdateAsync(InvoiceRecord invoice, CancellationToken cancellationToken = default);
        Task UpdateStatusAsync(Guid id, InvoiceStatus status, string? message = null, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<int> GetNextInvoiceNumberAsync(Guid senderId, int year, CancellationToken cancellationToken = default);
        Task<(IEnumerable<InvoiceRecord> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, 
            int pageSize, 
            Guid? clientId = null,
            InvoiceStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default);
    }
}
