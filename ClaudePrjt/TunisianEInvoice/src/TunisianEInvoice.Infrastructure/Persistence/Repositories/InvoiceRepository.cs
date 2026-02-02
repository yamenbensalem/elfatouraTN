using Microsoft.EntityFrameworkCore;
using TunisianEInvoice.Application.Interfaces;
using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.Infrastructure.Persistence.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly EInvoiceDbContext _context;

        public InvoiceRepository(EInvoiceDbContext context)
        {
            _context = context;
        }

        public async Task<InvoiceRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Invoices
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        }

        public async Task<InvoiceRecord?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Sender)
                .Include(i => i.Receiver)
                .Include(i => i.LineItems.OrderBy(l => l.LineNumber))
                .Include(i => i.Taxes)
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<InvoiceRecord>> GetByClientAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            return await _context.Invoices
                .AsNoTracking()
                .Where(i => i.SenderId == clientId || i.ReceiverId == clientId)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<InvoiceRecord>> GetByStatusAsync(InvoiceStatus status, CancellationToken cancellationToken = default)
        {
            return await _context.Invoices
                .AsNoTracking()
                .Where(i => i.Status == status)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<InvoiceRecord>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
        {
            return await _context.Invoices
                .AsNoTracking()
                .Where(i => i.InvoiceDate >= from && i.InvoiceDate <= to)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<InvoiceRecord?> GetByTtnReferenceAsync(string ttnReference, CancellationToken cancellationToken = default)
        {
            return await _context.Invoices
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.TtnReference == ttnReference, cancellationToken);
        }

        public async Task<InvoiceRecord> AddAsync(InvoiceRecord invoice, CancellationToken cancellationToken = default)
        {
            invoice.CreatedAt = DateTime.UtcNow;
            await _context.Invoices.AddAsync(invoice, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return invoice;
        }

        public async Task UpdateAsync(InvoiceRecord invoice, CancellationToken cancellationToken = default)
        {
            invoice.UpdatedAt = DateTime.UtcNow;
            _context.Invoices.Update(invoice);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateStatusAsync(Guid id, InvoiceStatus status, string? message = null, CancellationToken cancellationToken = default)
        {
            var invoice = await _context.Invoices.FindAsync(new object[] { id }, cancellationToken);
            if (invoice != null)
            {
                invoice.Status = status;
                invoice.StatusMessage = message;
                invoice.UpdatedAt = DateTime.UtcNow;
                
                if (status == InvoiceStatus.Validated)
                    invoice.ValidatedAt = DateTime.UtcNow;
                else if (status == InvoiceStatus.Sent)
                    invoice.SentAt = DateTime.UtcNow;
                else if (status == InvoiceStatus.Signed)
                    invoice.SignedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var invoice = await _context.Invoices.FindAsync(new object[] { id }, cancellationToken);
            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<int> GetNextInvoiceNumberAsync(Guid senderId, int year, CancellationToken cancellationToken = default)
        {
            var maxNumber = await _context.Invoices
                .Where(i => i.SenderId == senderId && i.InvoiceDate.Year == year)
                .MaxAsync(i => (int?)i.InvoiceNumber, cancellationToken);

            return (maxNumber ?? 0) + 1;
        }

        public async Task<(IEnumerable<InvoiceRecord> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Guid? clientId = null,
            InvoiceStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Invoices.AsNoTracking();

            if (clientId.HasValue)
                query = query.Where(i => i.SenderId == clientId.Value || i.ReceiverId == clientId.Value);

            if (status.HasValue)
                query = query.Where(i => i.Status == status.Value);

            if (fromDate.HasValue)
                query = query.Where(i => i.InvoiceDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(i => i.InvoiceDate <= toDate.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(i => i.InvoiceDate)
                .ThenByDescending(i => i.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(i => i.Sender)
                .Include(i => i.Receiver)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
