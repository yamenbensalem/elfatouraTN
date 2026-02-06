using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class ReglementFactureConfiguration : IEntityTypeConfiguration<ReglementFacture>
{
    public void Configure(EntityTypeBuilder<ReglementFacture> builder)
    {
        builder.ToTable("ReglementFacture");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("id");

        builder.Property(r => r.CodeEntreprise)
            .HasMaxLength(50)
            .HasColumnName("code_entreprise");

        builder.Property(r => r.NumeroFacture)
            .HasMaxLength(50)
            .HasColumnName("numero_facture");

        builder.Property(r => r.CodeClient)
            .HasMaxLength(50)
            .HasColumnName("code_client");

        builder.Property(r => r.DateReglement)
            .HasColumnName("date_reglement");

        builder.Property(r => r.Montant)
            .HasPrecision(18, 3)
            .HasColumnName("montant");

        builder.Property(r => r.ModePayement)
            .HasMaxLength(50)
            .HasColumnName("mode_payement");

        builder.Property(r => r.NumeroTransaction)
            .HasMaxLength(100)
            .HasColumnName("numero_transaction");

        builder.Property(r => r.Notes)
            .HasMaxLength(500)
            .HasColumnName("notes");

        // Relationships
        builder.HasOne(r => r.Entreprise)
            .WithMany()
            .HasForeignKey(r => r.CodeEntreprise)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.FactureClient)
            .WithMany(f => f.Reglements)
            .HasForeignKey(r => r.NumeroFacture)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Client)
            .WithMany()
            .HasForeignKey(r => r.CodeClient)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.CodeEntreprise);
        builder.HasIndex(r => r.NumeroFacture);
        builder.HasIndex(r => r.CodeClient);
        builder.HasIndex(r => r.DateReglement);
    }
}
