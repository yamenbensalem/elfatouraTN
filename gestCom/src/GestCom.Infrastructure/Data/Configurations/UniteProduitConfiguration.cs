using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class UniteProduitConfiguration : IEntityTypeConfiguration<UniteProduit>
{
    public void Configure(EntityTypeBuilder<UniteProduit> builder)
    {
        builder.ToTable("UniteProduit");

        builder.HasKey(u => u.CodeUnite);

        builder.Property(u => u.CodeUnite)
            .ValueGeneratedOnAdd()
            .HasColumnName("code_unite");

        builder.Property(u => u.Designation)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("designation");

        builder.Property(u => u.Symbole)
            .HasMaxLength(20)
            .HasColumnName("symbole");

        // Ignore alias properties
        builder.Ignore(u => u.Libelle);
    }
}
