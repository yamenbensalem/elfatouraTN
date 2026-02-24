using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class CommandeVenteConfiguration : IEntityTypeConfiguration<CommandeVente>
{
    public void Configure(EntityTypeBuilder<CommandeVente> builder)
    {
        builder.ToTable("CommandeVente");

        builder.HasKey(c => c.NumeroCommande);

        builder.Property(c => c.NumeroCommande).HasMaxLength(50);
        builder.Property(c => c.CodeEntreprise).HasMaxLength(50);
        builder.Property(c => c.CodeClient).HasMaxLength(50);
        builder.Property(c => c.CodeDevise).HasMaxLength(50);
        builder.Property(c => c.Observations).HasMaxLength(1000);
        builder.Property(c => c.Observation).HasMaxLength(1000);
        builder.Property(c => c.Notes).HasMaxLength(500);
        builder.Property(c => c.AdresseLivraison).HasMaxLength(500);
        builder.Property(c => c.NumeroDevis).HasMaxLength(50);
        builder.Property(c => c.NumeroBonLivraison).HasMaxLength(50);
        builder.Property(c => c.Statut).HasMaxLength(50);

        builder.Property(c => c.Remise).HasPrecision(18, 3);
        builder.Property(c => c.TauxRemise).HasPrecision(18, 3);
        builder.Property(c => c.TauxRemiseGlobale).HasPrecision(18, 3);
        builder.Property(c => c.MontantRemise).HasPrecision(18, 3);
        builder.Property(c => c.TauxChange).HasPrecision(18, 6);
        builder.Property(c => c.MontantHT).HasPrecision(18, 3);
        builder.Property(c => c.MontantTVA).HasPrecision(18, 3);
        builder.Property(c => c.MontantTTC).HasPrecision(18, 3);

        // Relationships
        builder.HasOne(c => c.Entreprise)
            .WithMany()
            .HasForeignKey(c => c.CodeEntreprise)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Client)
            .WithMany(cl => cl.Commandes)
            .HasForeignKey(c => c.CodeClient)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Devis)
            .WithMany()
            .HasForeignKey(c => c.NumeroDevis)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Lignes)
            .WithOne(l => l.CommandeVente)
            .HasForeignKey(l => l.NumeroCommande)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.BonsLivraison)
            .WithOne(b => b.CommandeVente)
            .HasForeignKey(b => b.NumeroCommande)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => c.CodeEntreprise);
        builder.HasIndex(c => c.CodeClient);
        builder.HasIndex(c => c.DateCommande);
    }
}

public class LigneCommandeVenteConfiguration : IEntityTypeConfiguration<LigneCommandeVente>
{
    public void Configure(EntityTypeBuilder<LigneCommandeVente> builder)
    {
        builder.ToTable("LigneCommandeVente");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id).ValueGeneratedOnAdd();
        builder.Property(l => l.NumeroCommande).HasMaxLength(50);
        builder.Property(l => l.CodeProduit).HasMaxLength(50);
        builder.Property(l => l.Designation).HasMaxLength(200);

        builder.Property(l => l.Quantite).HasPrecision(18, 3);
        builder.Property(l => l.QuantiteLivree).HasPrecision(18, 3);
        builder.Property(l => l.PrixUnitaire).HasPrecision(18, 3);
        builder.Property(l => l.PrixUnitaireHT).HasPrecision(18, 3);
        builder.Property(l => l.Remise).HasPrecision(18, 3);
        builder.Property(l => l.TauxRemise).HasPrecision(18, 3);
        builder.Property(l => l.MontantRemise).HasPrecision(18, 3);
        builder.Property(l => l.TauxFodec).HasPrecision(18, 3);
        builder.Property(l => l.MontantFodec).HasPrecision(18, 3);
        builder.Property(l => l.MontantHT).HasPrecision(18, 3);
        builder.Property(l => l.TauxTVA).HasPrecision(18, 3);
        builder.Property(l => l.MontantTVA).HasPrecision(18, 3);
        builder.Property(l => l.MontantTTC).HasPrecision(18, 3);

        builder.HasOne(l => l.Produit)
            .WithMany()
            .HasForeignKey(l => l.CodeProduit)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.NumeroCommande);
        builder.HasIndex(l => l.CodeProduit);
    }
}
