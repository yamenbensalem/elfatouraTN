using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class BonReceptionConfiguration : IEntityTypeConfiguration<BonReception>
{
    public void Configure(EntityTypeBuilder<BonReception> builder)
    {
        builder.ToTable("BonReception");

        builder.HasKey(b => b.NumeroBon); // Simple key

        builder.Property(b => b.NumeroBon).HasMaxLength(50);
        builder.Property(b => b.CodeEntreprise).HasMaxLength(50);
        builder.Property(b => b.CodeFournisseur).HasMaxLength(50);
        builder.Property(b => b.Notes).HasMaxLength(500);
        builder.Property(b => b.NumeroCommande).HasMaxLength(50);
        builder.Property(b => b.NumeroFacture).HasMaxLength(50);
        builder.Property(b => b.Statut).HasMaxLength(50);

        builder.Property(b => b.MontantHT).HasPrecision(18, 3);
        builder.Property(b => b.MontantTVA).HasPrecision(18, 3);
        builder.Property(b => b.MontantTTC).HasPrecision(18, 3);

        // Ignore alias properties
        builder.Ignore(b => b.NumeroBonReception);
        builder.Ignore(b => b.LignesBonReception);

        // Relationships
        builder.HasOne(b => b.Entreprise)
            .WithMany()
            .HasForeignKey(b => b.CodeEntreprise)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Fournisseur)
            .WithMany(f => f.BonsReception)
            .HasForeignKey(b => b.CodeFournisseur)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Lignes)
            .WithOne(l => l.BonReception)
            .HasForeignKey(l => l.NumeroBon)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => b.CodeEntreprise);
        builder.HasIndex(b => b.CodeFournisseur);
        builder.HasIndex(b => b.DateReception);
    }
}

public class LigneBonReceptionConfiguration : IEntityTypeConfiguration<LigneBonReception>
{
    public void Configure(EntityTypeBuilder<LigneBonReception> builder)
    {
        builder.ToTable("LigneBonReception");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id).ValueGeneratedOnAdd();
        builder.Property(l => l.NumeroBon).HasMaxLength(50);
        builder.Property(l => l.CodeProduit).HasMaxLength(50);
        builder.Property(l => l.Designation).HasMaxLength(200);

        builder.Property(l => l.Quantite).HasPrecision(18, 3);
        builder.Property(l => l.PrixUnitaire).HasPrecision(18, 3);
        builder.Property(l => l.Remise).HasPrecision(18, 3);
        builder.Property(l => l.MontantHT).HasPrecision(18, 3);
        builder.Property(l => l.TauxTVA).HasPrecision(18, 3);
        builder.Property(l => l.MontantTVA).HasPrecision(18, 3);
        builder.Property(l => l.MontantTTC).HasPrecision(18, 3);

        builder.HasOne(l => l.Produit)
            .WithMany()
            .HasForeignKey(l => l.CodeProduit)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.NumeroBon);
        builder.HasIndex(l => l.CodeProduit);
    }
}
