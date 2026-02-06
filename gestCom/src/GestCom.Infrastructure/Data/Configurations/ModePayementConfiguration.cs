using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class ModePayementConfiguration : IEntityTypeConfiguration<ModePayement>
{
    public void Configure(EntityTypeBuilder<ModePayement> builder)
    {
        builder.ToTable("ModePayement");

        builder.HasKey(m => m.CodeMode);

        builder.Property(m => m.CodeMode)
            .HasMaxLength(50)
            .HasColumnName("code_mode");

        builder.Property(m => m.Designation)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("designation");

        builder.Property(m => m.Description)
            .HasMaxLength(500)
            .HasColumnName("description");

        builder.Property(m => m.Actif)
            .HasColumnName("actif");

        // Ignore alias properties
        builder.Ignore(m => m.LibelleMode);
    }
}
