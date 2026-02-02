using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.Infrastructure.Persistence.Configurations
{
    public class InvoiceRecordConfiguration : IEntityTypeConfiguration<InvoiceRecord>
    {
        public void Configure(EntityTypeBuilder<InvoiceRecord> builder)
        {
            builder.ToTable("Invoices");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.DocumentIdentifier)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(i => new { i.SenderId, i.DocumentIdentifier })
                .IsUnique();

            builder.Property(i => i.DocumentTypeCode)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(i => i.DocumentTypeName)
                .HasMaxLength(50);

            builder.Property(i => i.PeriodFrom)
                .HasMaxLength(20);

            builder.Property(i => i.PeriodTo)
                .HasMaxLength(20);

            // Amounts
            builder.Property(i => i.TotalExcludingTax)
                .HasPrecision(18, 3);

            builder.Property(i => i.TotalTaxAmount)
                .HasPrecision(18, 3);

            builder.Property(i => i.StampDuty)
                .HasPrecision(18, 3);

            builder.Property(i => i.TotalIncludingTax)
                .HasPrecision(18, 3);

            builder.Property(i => i.AmountInWords)
                .HasMaxLength(500);

            // Status
            builder.Property(i => i.Status)
                .HasConversion<int>();

            builder.Property(i => i.StatusMessage)
                .HasMaxLength(500);

            // TTN
            builder.Property(i => i.TtnReference)
                .HasMaxLength(50);

            // XML stored as large text
            builder.Property(i => i.XmlWithoutSignature)
                .HasColumnType("nvarchar(max)");

            builder.Property(i => i.XmlWithSignature)
                .HasColumnType("nvarchar(max)");

            builder.Property(i => i.CreatedBy)
                .HasMaxLength(100);

            builder.Property(i => i.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(i => i.Sender)
                .WithMany(c => c.SentInvoices)
                .HasForeignKey(i => i.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.Receiver)
                .WithMany(c => c.ReceivedInvoices)
                .HasForeignKey(i => i.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(i => i.Status);
            builder.HasIndex(i => i.InvoiceDate);
            builder.HasIndex(i => i.TtnReference);
        }
    }
}
