using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class BonLivraisonConfiguration : IEntityTypeConfiguration<BonLivraison>
{
    public void Configure(EntityTypeBuilder<BonLivraison> builder)
    {
        builder.HasKey(b => b.NumeroBon);

        builder.Property(b => b.NumeroBon).HasMaxLength(50);
        builder.Property(b => b.CodeEntreprise).HasMaxLength(50);
        builder.Property(b => b.CodeClient).HasMaxLength(50);
        builder.Property(b => b.Statut).HasMaxLength(50);
        builder.Property(b => b.Notes).HasMaxLength(500);
        builder.Property(b => b.Observations).HasMaxLength(500);
        builder.Property(b => b.AdresseLivraison).HasMaxLength(500);
        builder.Property(b => b.NumeroCommande).HasMaxLength(50);
        builder.Property(b => b.NumeroFacture).HasMaxLength(50);

        builder.Property(b => b.MontantHT).HasPrecision(18, 3);
        builder.Property(b => b.MontantTVA).HasPrecision(18, 3);
        builder.Property(b => b.MontantTTC).HasPrecision(18, 3);

        // Ignore alias properties that point to the same data
        builder.Ignore(b => b.NumeroBonLivraison);
        builder.Ignore(b => b.DateBonLivraison);

        // One-to-many: BonLivraison -> Lignes
        builder.HasMany(b => b.Lignes)
            .WithOne(l => l.BonLivraison)
            .HasForeignKey(l => l.NumeroBon)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Client)
            .WithMany(c => c.BonsLivraison)
            .HasForeignKey(b => b.CodeClient)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => b.CodeEntreprise);
        builder.HasIndex(b => b.CodeClient);
        builder.HasIndex(b => b.DateLivraison);
    }
}

public class LigneBonLivraisonConfiguration : IEntityTypeConfiguration<LigneBonLivraison>
{
    public void Configure(EntityTypeBuilder<LigneBonLivraison> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.NumeroBon).HasMaxLength(50);
        builder.Property(l => l.NumeroBonLivraison).HasMaxLength(50);
        builder.Property(l => l.CodeProduit).HasMaxLength(50);
        builder.Property(l => l.Designation).HasMaxLength(200);

        builder.Property(l => l.Quantite).HasPrecision(18, 3);
        builder.Property(l => l.PrixUnitaire).HasPrecision(18, 3);
        builder.Property(l => l.PrixUnitaireHT).HasPrecision(18, 3);
        builder.Property(l => l.Remise).HasPrecision(18, 3);
        builder.Property(l => l.MontantHT).HasPrecision(18, 3);
        builder.Property(l => l.TauxTVA).HasPrecision(18, 3);
        builder.Property(l => l.MontantTVA).HasPrecision(18, 3);
        builder.Property(l => l.MontantTTC).HasPrecision(18, 3);
        builder.Property(l => l.TauxRemise).HasPrecision(18, 3);
        builder.Property(l => l.TauxFodec).HasPrecision(18, 3);
        builder.Property(l => l.MontantFodec).HasPrecision(18, 3);

        builder.HasOne(l => l.Produit)
            .WithMany()
            .HasForeignKey(l => l.CodeProduit)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.NumeroBon);
        builder.HasIndex(l => l.CodeProduit);
    }
}
