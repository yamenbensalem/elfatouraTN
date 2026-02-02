using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.Infrastructure.Persistence.Configurations
{
    public class InvoiceTaxRecordConfiguration : IEntityTypeConfiguration<InvoiceTaxRecord>
    {
        public void Configure(EntityTypeBuilder<InvoiceTaxRecord> builder)
        {
            builder.ToTable("InvoiceTaxes");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.TaxTypeCode)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(t => t.TaxTypeName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.TaxRate)
                .HasPrecision(5, 2);

            builder.Property(t => t.TaxableBase)
                .HasPrecision(18, 3);

            builder.Property(t => t.TaxAmount)
                .HasPrecision(18, 3);

            // Relationship
            builder.HasOne(t => t.Invoice)
                .WithMany(i => i.Taxes)
                .HasForeignKey(t => t.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(t => new { t.InvoiceId, t.TaxTypeCode });
        }
    }
}
