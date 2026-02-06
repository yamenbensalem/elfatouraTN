using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class ReglementFournisseurConfiguration : IEntityTypeConfiguration<ReglementFournisseur>
{
    public void Configure(EntityTypeBuilder<ReglementFournisseur> builder)
    {
        builder.ToTable("ReglementFournisseur");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).ValueGeneratedOnAdd();
        builder.Property(r => r.CodeEntreprise).HasMaxLength(50);
        builder.Property(r => r.NumeroFacture).HasMaxLength(50);
        builder.Property(r => r.CodeFournisseur).HasMaxLength(50);
        builder.Property(r => r.ModePayement).HasMaxLength(50);
        builder.Property(r => r.NumeroTransaction).HasMaxLength(100);
        builder.Property(r => r.Notes).HasMaxLength(500);

        builder.Property(r => r.Montant).HasPrecision(18, 3);

        // Relationships
        builder.HasOne(r => r.Entreprise)
            .WithMany()
            .HasForeignKey(r => r.CodeEntreprise)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Fournisseur)
            .WithMany(f => f.Reglements)
            .HasForeignKey(r => r.CodeFournisseur)
            .OnDelete(DeleteBehavior.Restrict);

        // Note: FactureFournisseur relationship is configured in FactureFournisseurConfiguration

        builder.HasIndex(r => r.CodeEntreprise);
        builder.HasIndex(r => r.NumeroFacture);
        builder.HasIndex(r => r.CodeFournisseur);
        builder.HasIndex(r => r.DateReglement);
    }
}
