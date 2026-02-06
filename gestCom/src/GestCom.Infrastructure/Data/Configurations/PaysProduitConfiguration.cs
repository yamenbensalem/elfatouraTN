using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class PaysProduitConfiguration : IEntityTypeConfiguration<PaysProduit>
{
    public void Configure(EntityTypeBuilder<PaysProduit> builder)
    {
        builder.ToTable("PaysProduit");

        builder.HasKey(p => p.CodePays);

        builder.Property(p => p.CodePays)
            .ValueGeneratedOnAdd()
            .HasColumnName("code_pays");

        builder.Property(p => p.Designation)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("designation");

        builder.Property(p => p.CodeISO)
            .HasMaxLength(10)
            .HasColumnName("code_iso");
    }
}
