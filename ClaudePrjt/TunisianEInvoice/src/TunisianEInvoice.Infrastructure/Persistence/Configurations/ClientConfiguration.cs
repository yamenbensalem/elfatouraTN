using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.Infrastructure.Persistence.Configurations
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.ToTable("Clients");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.MatriculeFiscal)
                .IsRequired()
                .HasMaxLength(25);

            builder.HasIndex(c => c.MatriculeFiscal)
                .IsUnique();

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.LegalForm)
                .HasMaxLength(50);

            builder.Property(c => c.RegistrationNumber)
                .HasMaxLength(50);

            builder.Property(c => c.Capital)
                .HasPrecision(18, 3);

            builder.Property(c => c.AddressDescription)
                .HasMaxLength(500);

            builder.Property(c => c.Street)
                .HasMaxLength(200);

            builder.Property(c => c.City)
                .HasMaxLength(100);

            builder.Property(c => c.PostalCode)
                .HasMaxLength(10);

            builder.Property(c => c.CountryCode)
                .HasMaxLength(2)
                .HasDefaultValue("TN");

            builder.Property(c => c.Phone)
                .HasMaxLength(20);

            builder.Property(c => c.Fax)
                .HasMaxLength(20);

            builder.Property(c => c.Email)
                .HasMaxLength(100);

            builder.Property(c => c.Website)
                .HasMaxLength(200);

            builder.Property(c => c.BankAccountNumber)
                .HasMaxLength(50);

            builder.Property(c => c.BankCode)
                .HasMaxLength(10);

            builder.Property(c => c.BankName)
                .HasMaxLength(100);

            builder.Property(c => c.TtnAccountMode)
                .HasMaxLength(20);

            builder.Property(c => c.TtnAccountRank)
                .HasMaxLength(5);

            builder.Property(c => c.TtnProfile)
                .HasMaxLength(50);

            builder.Property(c => c.TtnClientCode)
                .HasMaxLength(20);

            builder.Property(c => c.CertificatePassword)
                .HasMaxLength(100);

            builder.Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
