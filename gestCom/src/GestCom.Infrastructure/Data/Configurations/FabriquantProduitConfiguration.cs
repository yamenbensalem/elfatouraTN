using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class FabriquantProduitConfiguration : IEntityTypeConfiguration<FabriquantProduit>
{
    public void Configure(EntityTypeBuilder<FabriquantProduit> builder)
    {
        builder.ToTable("FabriquantProduit");

        builder.HasKey(f => f.CodeFabriquant);

        builder.Property(f => f.CodeFabriquant)
            .ValueGeneratedOnAdd()
            .HasColumnName("code_fabriquant");

        builder.Property(f => f.Designation)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("designation");

        builder.Property(f => f.Pays)
            .HasMaxLength(100)
            .HasColumnName("pays");

        builder.Property(f => f.SiteWeb)
            .HasMaxLength(200)
            .HasColumnName("site_web");
    }
}
