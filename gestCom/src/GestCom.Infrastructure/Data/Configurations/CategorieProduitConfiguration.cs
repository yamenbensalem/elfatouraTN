using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class CategorieProduitConfiguration : IEntityTypeConfiguration<CategorieProduit>
{
    public void Configure(EntityTypeBuilder<CategorieProduit> builder)
    {
        builder.ToTable("CategorieProduit");

        builder.HasKey(c => c.CodeCategorie);

        builder.Property(c => c.CodeCategorie)
            .ValueGeneratedOnAdd()
            .HasColumnName("code_categorie");

        builder.Property(c => c.CodeEntreprise)
            .HasMaxLength(50)
            .HasColumnName("code_entreprise");

        builder.Property(c => c.Designation)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("designation");

        builder.Property(c => c.Description)
            .HasMaxLength(500)
            .HasColumnName("description");

        builder.Property(c => c.CategorieParentId)
            .HasColumnName("categorie_parent_id");

        // Ignore alias properties
        builder.Ignore(c => c.Libelle);
        builder.Ignore(c => c.LibelleCategorie);

        // Self-referencing relationship for hierarchical categories
        builder.HasOne(c => c.CategorieParent)
            .WithMany(c => c.SousCategories)
            .HasForeignKey(c => c.CategorieParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Entreprise)
            .WithMany()
            .HasForeignKey(c => c.CodeEntreprise)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.CodeEntreprise);
        builder.HasIndex(c => c.CategorieParentId);
    }
}
