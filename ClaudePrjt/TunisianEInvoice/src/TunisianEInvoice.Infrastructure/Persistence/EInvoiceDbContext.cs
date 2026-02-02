using Microsoft.EntityFrameworkCore;
using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.Infrastructure.Persistence
{
    public class EInvoiceDbContext : DbContext
    {
        public EInvoiceDbContext(DbContextOptions<EInvoiceDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients => Set<Client>();
        public DbSet<InvoiceRecord> Invoices => Set<InvoiceRecord>();
        public DbSet<InvoiceLineRecord> InvoiceLines => Set<InvoiceLineRecord>();
        public DbSet<InvoiceTaxRecord> InvoiceTaxes => Set<InvoiceTaxRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configurations
            modelBuilder.ApplyConfiguration(new Configurations.ClientConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.InvoiceRecordConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.InvoiceLineRecordConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.InvoiceTaxRecordConfiguration());
        }
    }
}
