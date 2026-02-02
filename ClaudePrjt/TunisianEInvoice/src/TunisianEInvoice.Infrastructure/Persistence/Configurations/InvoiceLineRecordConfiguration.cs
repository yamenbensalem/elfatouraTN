using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.Infrastructure.Persistence.Configurations
{
    public class InvoiceLineRecordConfiguration : IEntityTypeConfiguration<InvoiceLineRecord>
    {
        public void Configure(EntityTypeBuilder<InvoiceLineRecord> builder)
        {
            builder.ToTable("InvoiceLines");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.ItemCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(l => l.ItemDescription)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(l => l.Language)
                .HasMaxLength(5)
                .HasDefaultValue("fr");

            builder.Property(l => l.MeasurementUnit)
                .HasMaxLength(20)
                .HasDefaultValue("UNIT");

            builder.Property(l => l.Quantity)
                .HasPrecision(18, 3);

            builder.Property(l => l.UnitPriceExcludingTax)
                .HasPrecision(18, 3);

            builder.Property(l => l.TotalExcludingTax)
                .HasPrecision(18, 3);

            builder.Property(l => l.TaxTypeCode)
                .HasMaxLength(10);

            builder.Property(l => l.TaxTypeName)
                .HasMaxLength(50);

            builder.Property(l => l.TaxRate)
                .HasPrecision(5, 2);

            builder.Property(l => l.TaxAmount)
                .HasPrecision(18, 3);

            // Relationship
            builder.HasOne(l => l.Invoice)
                .WithMany(i => i.LineItems)
                .HasForeignKey(l => l.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(l => new { l.InvoiceId, l.LineNumber });
        }
    }
}
