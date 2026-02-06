using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class FactureFournisseurConfiguration : IEntityTypeConfiguration<FactureFournisseur>
{
    public void Configure(EntityTypeBuilder<FactureFournisseur> builder)
    {
        builder.ToTable("FactureFournisseur");

        builder.HasKey(f => f.NumeroFacture);

        builder.Property(f => f.NumeroFacture).HasMaxLength(50);
        builder.Property(f => f.CodeEntreprise).HasMaxLength(50);
        builder.Property(f => f.CodeFournisseur).HasMaxLength(50);
        builder.Property(f => f.NumeroBonReception).HasMaxLength(50);
        builder.Property(f => f.Observation).HasMaxLength(1000);
        builder.Property(f => f.ModePayement).HasMaxLength(50);
        builder.Property(f => f.Notes).HasMaxLength(500);
        builder.Property(f => f.NumeroFactureFournisseur).HasMaxLength(100);
        builder.Property(f => f.Statut).HasMaxLength(50);

        builder.Property(f => f.Remise).HasPrecision(18, 3);
        builder.Property(f => f.TauxRemiseGlobale).HasPrecision(18, 3);
        builder.Property(f => f.MontantRemise).HasPrecision(18, 3);
        builder.Property(f => f.MontantHT).HasPrecision(18, 3);
        builder.Property(f => f.MontantTVA).HasPrecision(18, 3);
        builder.Property(f => f.Timbre).HasPrecision(18, 3);
        builder.Property(f => f.MontantTTC).HasPrecision(18, 3);
        builder.Property(f => f.APayer).HasPrecision(18, 3);
        builder.Property(f => f.MontantRegle).HasPrecision(18, 3);
        builder.Property(f => f.MontantRestant).HasPrecision(18, 3);
        builder.Property(f => f.MontantFodec).HasPrecision(18, 3);
        builder.Property(f => f.TauxRAS).HasPrecision(18, 3);
        builder.Property(f => f.MontantRAS).HasPrecision(18, 3);
        builder.Property(f => f.NetAPayer).HasPrecision(18, 3);

        // Ignore alias collections
        builder.Ignore(f => f.LignesFacture);

        // Relationships
        builder.HasOne(f => f.Entreprise)
            .WithMany()
            .HasForeignKey(f => f.CodeEntreprise)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Fournisseur)
            .WithMany(fo => fo.Factures)
            .HasForeignKey(f => f.CodeFournisseur)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(f => f.Lignes)
            .WithOne(l => l.FactureFournisseur)
            .HasForeignKey(l => l.NumeroFacture)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(f => f.Reglements)
            .WithOne(r => r.FactureFournisseur)
            .HasForeignKey(r => r.NumeroFacture)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => f.CodeEntreprise);
        builder.HasIndex(f => f.CodeFournisseur);
        builder.HasIndex(f => f.DateFacture);
    }
}

public class LigneFactureFournisseurConfiguration : IEntityTypeConfiguration<LigneFactureFournisseur>
{
    public void Configure(EntityTypeBuilder<LigneFactureFournisseur> builder)
    {
        builder.ToTable("LigneFactureFournisseur");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id).ValueGeneratedOnAdd();
        builder.Property(l => l.NumeroFacture).HasMaxLength(50);
        builder.Property(l => l.CodeProduit).HasMaxLength(50);
        builder.Property(l => l.Designation).HasMaxLength(200);

        builder.Property(l => l.Quantite).HasPrecision(18, 3);
        builder.Property(l => l.PrixUnitaire).HasPrecision(18, 3);
        builder.Property(l => l.PrixUnitaireHT).HasPrecision(18, 3);
        builder.Property(l => l.Remise).HasPrecision(18, 3);
        builder.Property(l => l.TauxRemise).HasPrecision(18, 3);
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

        builder.HasIndex(l => l.NumeroFacture);
        builder.HasIndex(l => l.CodeProduit);
    }
}
