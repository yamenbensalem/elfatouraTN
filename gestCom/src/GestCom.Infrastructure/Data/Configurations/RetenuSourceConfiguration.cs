using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class RetenuSourceConfiguration : IEntityTypeConfiguration<RetenuSource>
{
    public void Configure(EntityTypeBuilder<RetenuSource> builder)
    {
        builder.ToTable("RetenuSource");

        builder.HasKey(r => r.CodeRetenu);

        builder.Property(r => r.CodeRetenu)
            .ValueGeneratedOnAdd()
            .HasColumnName("code_retenu");

        builder.Property(r => r.Designation)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("designation");

        builder.Property(r => r.Taux)
            .HasPrecision(18, 3)
            .HasColumnName("taux");

        builder.Property(r => r.Description)
            .HasMaxLength(500)
            .HasColumnName("description");
    }
}
