using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class MagasinProduitConfiguration : IEntityTypeConfiguration<MagasinProduit>
{
    public void Configure(EntityTypeBuilder<MagasinProduit> builder)
    {
        builder.ToTable("MagasinProduit");

        builder.HasKey(m => m.CodeMagasin);

        builder.Property(m => m.CodeMagasin)
            .ValueGeneratedOnAdd()
            .HasColumnName("code_magasin");

        builder.Property(m => m.CodeEntreprise)
            .HasMaxLength(50)
            .HasColumnName("code_entreprise");

        builder.Property(m => m.Designation)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("designation");

        builder.Property(m => m.Adresse)
            .HasMaxLength(500)
            .HasColumnName("adresse");

        builder.Property(m => m.Ville)
            .HasMaxLength(100)
            .HasColumnName("ville");

        builder.Property(m => m.Responsable)
            .HasMaxLength(200)
            .HasColumnName("responsable");

        builder.Property(m => m.Principal)
            .HasColumnName("principal");

        // Ignore alias properties
        builder.Ignore(m => m.Libelle);

        builder.HasOne(m => m.Entreprise)
            .WithMany()
            .HasForeignKey(m => m.CodeEntreprise)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.CodeEntreprise);
    }
}
