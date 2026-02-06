using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class DemandePrixConfiguration : IEntityTypeConfiguration<DemandePrix>
{
    public void Configure(EntityTypeBuilder<DemandePrix> builder)
    {
        builder.ToTable("DemandePrix");

        builder.HasKey(d => d.NumeroDemande);

        builder.Property(d => d.NumeroDemande).HasMaxLength(50);
        builder.Property(d => d.CodeEntreprise).HasMaxLength(50);
        builder.Property(d => d.CodeFournisseur).HasMaxLength(50);
        builder.Property(d => d.Notes).HasMaxLength(500);
        builder.Property(d => d.Statut).HasMaxLength(50);

        // Relationships
        builder.HasOne(d => d.Entreprise)
            .WithMany()
            .HasForeignKey(d => d.CodeEntreprise)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Fournisseur)
            .WithMany(f => f.DemandesPrix)
            .HasForeignKey(d => d.CodeFournisseur)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.Lignes)
            .WithOne(l => l.DemandePrix)
            .HasForeignKey(l => l.NumeroDemande)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.CodeEntreprise);
        builder.HasIndex(d => d.CodeFournisseur);
        builder.HasIndex(d => d.DateDemande);
    }
}

public class LigneDemandePrixConfiguration : IEntityTypeConfiguration<LigneDemandePrix>
{
    public void Configure(EntityTypeBuilder<LigneDemandePrix> builder)
    {
        builder.ToTable("LigneDemandePrix");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id).ValueGeneratedOnAdd();
        builder.Property(l => l.NumeroDemande).HasMaxLength(50);
        builder.Property(l => l.CodeProduit).HasMaxLength(50);
        builder.Property(l => l.Designation).HasMaxLength(200);

        builder.Property(l => l.Quantite).HasPrecision(18, 3);
        builder.Property(l => l.PrixPropose).HasPrecision(18, 3);

        builder.HasOne(l => l.Produit)
            .WithMany()
            .HasForeignKey(l => l.CodeProduit)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.NumeroDemande);
        builder.HasIndex(l => l.CodeProduit);
    }
}
