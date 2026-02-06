using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class BonLivraisonFactureConfiguration : IEntityTypeConfiguration<BonLivraison_Facture>
{
    public void Configure(EntityTypeBuilder<BonLivraison_Facture> builder)
    {
        // Composite primary key
        builder.HasKey(bf => new { bf.NumeroBon, bf.NumeroFacture });

        builder.Property(bf => bf.NumeroBon).HasMaxLength(50);
        builder.Property(bf => bf.NumeroFacture).HasMaxLength(50);

        // Configure relationships
        builder.HasOne(bf => bf.BonLivraison)
            .WithMany(bl => bl.FacturesLiees)
            .HasForeignKey(bf => bf.NumeroBon)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bf => bf.FactureClient)
            .WithMany(f => f.BonsLivraisonLies)
            .HasForeignKey(bf => bf.NumeroFacture)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("BonsLivraison_Factures");
    }
}
