using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class DevisClientConfiguration : IEntityTypeConfiguration<DevisClient>
{
    public void Configure(EntityTypeBuilder<DevisClient> builder)
    {
        builder.ToTable("DevisClient");

        builder.HasKey(d => d.NumeroDevis);

        builder.Property(d => d.NumeroDevis).HasMaxLength(50);
        builder.Property(d => d.CodeEntreprise).HasMaxLength(50);
        builder.Property(d => d.CodeClient).HasMaxLength(50);
        builder.Property(d => d.CodeDevise).HasMaxLength(50);
        builder.Property(d => d.Observations).HasMaxLength(1000);
        builder.Property(d => d.Notes).HasMaxLength(500);
        builder.Property(d => d.NumeroCommande).HasMaxLength(50);
        builder.Property(d => d.Statut).HasMaxLength(50);

        builder.Property(d => d.Remise).HasPrecision(18, 3);
        builder.Property(d => d.TauxRemise).HasPrecision(18, 3);
        builder.Property(d => d.TauxRemiseGlobale).HasPrecision(18, 3);
        builder.Property(d => d.MontantRemise).HasPrecision(18, 3);
        builder.Property(d => d.Timbre).HasPrecision(18, 3);
        builder.Property(d => d.TauxChange).HasPrecision(18, 6);
        builder.Property(d => d.MontantHT).HasPrecision(18, 3);
        builder.Property(d => d.MontantTVA).HasPrecision(18, 3);
        builder.Property(d => d.MontantTTC).HasPrecision(18, 3);

        // Ignore alias properties
        builder.Ignore(d => d.Observation);

        // Relationships
        builder.HasOne(d => d.Entreprise)
            .WithMany()
            .HasForeignKey(d => d.CodeEntreprise)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Client)
            .WithMany(c => c.Devis)
            .HasForeignKey(d => d.CodeClient)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.Lignes)
            .WithOne(l => l.DevisClient)
            .HasForeignKey(l => l.NumeroDevis)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.CodeEntreprise);
        builder.HasIndex(d => d.CodeClient);
        builder.HasIndex(d => d.DateDevis);
    }
}

public class LigneDevisClientConfiguration : IEntityTypeConfiguration<LigneDevisClient>
{
    public void Configure(EntityTypeBuilder<LigneDevisClient> builder)
    {
        builder.ToTable("LigneDevisClient");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id).ValueGeneratedOnAdd();
        builder.Property(l => l.NumeroDevis).HasMaxLength(50);
        builder.Property(l => l.CodeProduit).HasMaxLength(50);
        builder.Property(l => l.Designation).HasMaxLength(200);

        builder.Property(l => l.Quantite).HasPrecision(18, 3);
        builder.Property(l => l.PrixUnitaire).HasPrecision(18, 3);
        builder.Property(l => l.PrixUnitaireHT).HasPrecision(18, 3);
        builder.Property(l => l.Remise).HasPrecision(18, 3);
        builder.Property(l => l.TauxRemise).HasPrecision(18, 3);
        builder.Property(l => l.MontantRemise).HasPrecision(18, 3);
        builder.Property(l => l.MontantHT).HasPrecision(18, 3);
        builder.Property(l => l.TauxTVA).HasPrecision(18, 3);
        builder.Property(l => l.MontantTVA).HasPrecision(18, 3);
        builder.Property(l => l.TauxFodec).HasPrecision(18, 3);
        builder.Property(l => l.MontantFodec).HasPrecision(18, 3);
        builder.Property(l => l.MontantTTC).HasPrecision(18, 3);

        builder.HasOne(l => l.Produit)
            .WithMany()
            .HasForeignKey(l => l.CodeProduit)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.NumeroDevis);
        builder.HasIndex(l => l.CodeProduit);
    }
}
