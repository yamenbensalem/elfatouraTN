using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class DeviseConfiguration : IEntityTypeConfiguration<Devise>
{
    public void Configure(EntityTypeBuilder<Devise> builder)
    {
        builder.ToTable("Devise");

        builder.HasKey(d => d.CodeDevise);

        builder.Property(d => d.CodeDevise)
            .ValueGeneratedOnAdd()
            .HasColumnName("code_devise");

        builder.Property(d => d.Nom)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("nom");

        builder.Property(d => d.Symbole)
            .HasMaxLength(10)
            .HasColumnName("symbole");

        builder.Property(d => d.CodeISO)
            .HasMaxLength(10)
            .HasColumnName("code_iso");

        builder.Property(d => d.TauxChange)
            .HasPrecision(18, 6)
            .HasColumnName("taux_change");

        builder.Property(d => d.DevisePrincipale)
            .HasColumnName("devise_principale");

        // Ignore alias properties
        builder.Ignore(d => d.LibelleDevise);

        builder.HasIndex(d => d.CodeISO).IsUnique();
    }
}
