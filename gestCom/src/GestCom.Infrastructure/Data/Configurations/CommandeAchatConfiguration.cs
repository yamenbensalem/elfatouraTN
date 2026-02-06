using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class CommandeAchatConfiguration : IEntityTypeConfiguration<CommandeAchat>
{
    public void Configure(EntityTypeBuilder<CommandeAchat> builder)
    {
        builder.ToTable("CommandeAchat");

        builder.HasKey(c => c.NumeroCommande);

        builder.Property(c => c.NumeroCommande).HasMaxLength(50);
        builder.Property(c => c.CodeEntreprise).HasMaxLength(50);
        builder.Property(c => c.CodeFournisseur).HasMaxLength(50);
        builder.Property(c => c.CodeDevise).HasMaxLength(50);
        builder.Property(c => c.Observations).HasMaxLength(1000);
        builder.Property(c => c.Notes).HasMaxLength(500);
        builder.Property(c => c.NumeroDemandePrix).HasMaxLength(50);
        builder.Property(c => c.NumeroBonReception).HasMaxLength(50);
        builder.Property(c => c.Statut).HasMaxLength(50);

        builder.Property(c => c.Remise).HasPrecision(18, 3);
        builder.Property(c => c.TauxRemise).HasPrecision(18, 3);
        builder.Property(c => c.TauxChange).HasPrecision(18, 6);
        builder.Property(c => c.MontantHT).HasPrecision(18, 3);
        builder.Property(c => c.MontantTVA).HasPrecision(18, 3);
        builder.Property(c => c.MontantTTC).HasPrecision(18, 3);

        // Ignore alias collections
        builder.Ignore(c => c.LignesCommande);

        // Relationships
        builder.HasOne(c => c.Entreprise)
            .WithMany()
            .HasForeignKey(c => c.CodeEntreprise)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Fournisseur)
            .WithMany(f => f.Commandes)
            .HasForeignKey(c => c.CodeFournisseur)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Lignes)
            .WithOne(l => l.CommandeAchat)
            .HasForeignKey(l => l.NumeroCommande)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.BonsReception)
            .WithOne(b => b.CommandeAchat)
            .HasForeignKey(b => b.NumeroCommande)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => c.CodeEntreprise);
        builder.HasIndex(c => c.CodeFournisseur);
        builder.HasIndex(c => c.DateCommande);
    }
}

public class LigneCommandeAchatConfiguration : IEntityTypeConfiguration<LigneCommandeAchat>
{
    public void Configure(EntityTypeBuilder<LigneCommandeAchat> builder)
    {
        builder.ToTable("LigneCommandeAchat");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id).ValueGeneratedOnAdd();
        builder.Property(l => l.NumeroCommande).HasMaxLength(50);
        builder.Property(l => l.CodeProduit).HasMaxLength(50);
        builder.Property(l => l.Designation).HasMaxLength(200);

        builder.Property(l => l.Quantite).HasPrecision(18, 3);
        builder.Property(l => l.QuantiteRecue).HasPrecision(18, 3);
        builder.Property(l => l.PrixUnitaire).HasPrecision(18, 3);
        builder.Property(l => l.PrixUnitaireHT).HasPrecision(18, 3);
        builder.Property(l => l.Remise).HasPrecision(18, 3);
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
