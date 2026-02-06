using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class FournisseurConfiguration : IEntityTypeConfiguration<Fournisseur>
{
    public void Configure(EntityTypeBuilder<Fournisseur> builder)
    {
        builder.ToTable("Fournisseur");

        builder.HasKey(f => f.CodeFournisseur);

        builder.Property(f => f.CodeFournisseur).HasMaxLength(50);
        builder.Property(f => f.CodeEntreprise).HasMaxLength(50);
        builder.Property(f => f.MatriculeFiscale).HasMaxLength(50);
        builder.Property(f => f.Nom).HasMaxLength(200);
        builder.Property(f => f.TypePersonne).HasMaxLength(50);
        builder.Property(f => f.TypeEntreprise).HasMaxLength(50);
        builder.Property(f => f.RIB).HasMaxLength(50);
        builder.Property(f => f.Adresse).HasMaxLength(500);
        builder.Property(f => f.CodePostal).HasMaxLength(20);
        builder.Property(f => f.Ville).HasMaxLength(100);
        builder.Property(f => f.Pays).HasMaxLength(100);
        builder.Property(f => f.Tel).HasMaxLength(20);
        builder.Property(f => f.TelMobile).HasMaxLength(20);
        builder.Property(f => f.Fax).HasMaxLength(20);
        builder.Property(f => f.Email).HasMaxLength(100);
        builder.Property(f => f.SiteWeb).HasMaxLength(200);
        builder.Property(f => f.Etat).HasMaxLength(50);
        builder.Property(f => f.Note).HasMaxLength(1000);
        builder.Property(f => f.Responsable).HasMaxLength(100);

        // Ignore alias properties
        builder.Ignore(f => f.RaisonSociale);
        builder.Ignore(f => f.NomFournisseur);
        builder.Ignore(f => f.Telephone);

        // Relationships
        builder.HasOne(f => f.Entreprise)
            .WithMany()
            .HasForeignKey(f => f.CodeEntreprise)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Devise)
            .WithMany()
            .HasForeignKey(f => f.CodeDevise)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(f => f.CodeEntreprise);
        builder.HasIndex(f => f.MatriculeFiscale);
    }
}
