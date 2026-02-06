using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class PathLogoConfiguration : IEntityTypeConfiguration<PathLogo>
{
    public void Configure(EntityTypeBuilder<PathLogo> builder)
    {
        builder.ToTable("PathLogo");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("id");

        builder.Property(p => p.CheminLogo)
            .HasMaxLength(500)
            .HasColumnName("chemin_logo");

        builder.Property(p => p.CheminRapport)
            .HasMaxLength(500)
            .HasColumnName("chemin_rapport");
    }
}
