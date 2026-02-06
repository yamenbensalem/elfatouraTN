using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class LicenceConfiguration : IEntityTypeConfiguration<Licence>
{
    public void Configure(EntityTypeBuilder<Licence> builder)
    {
        builder.ToTable("Licence");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("id");

        builder.Property(l => l.CodeEntreprise)
            .HasMaxLength(50)
            .HasColumnName("code_entreprise");

        builder.Property(l => l.CleLicence)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("cle_licence");

        builder.Property(l => l.DateDebut)
            .HasColumnName("date_debut");

        builder.Property(l => l.DateFin)
            .HasColumnName("date_fin");

        builder.Property(l => l.TypeLicence)
            .HasMaxLength(50)
            .HasColumnName("type_licence");

        builder.Property(l => l.NombreUtilisateurs)
            .HasColumnName("nombre_utilisateurs");

        builder.Property(l => l.Actif)
            .HasColumnName("actif");

        builder.HasIndex(l => l.CodeEntreprise);
        builder.HasIndex(l => l.CleLicence).IsUnique();
    }
}
