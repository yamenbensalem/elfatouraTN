using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class TvaProduitConfiguration : IEntityTypeConfiguration<TvaProduit>
{
    public void Configure(EntityTypeBuilder<TvaProduit> builder)
    {
        builder.ToTable("TvaProduit");

        builder.HasKey(t => t.CodeTVA);

        builder.Property(t => t.CodeTVA)
            .ValueGeneratedOnAdd()
            .HasColumnName("code_tva");

        builder.Property(t => t.Designation)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("designation");

        builder.Property(t => t.Taux)
            .HasPrecision(18, 3)
            .HasColumnName("taux");

        builder.Property(t => t.ParDefaut)
            .HasColumnName("par_defaut");
    }
}
