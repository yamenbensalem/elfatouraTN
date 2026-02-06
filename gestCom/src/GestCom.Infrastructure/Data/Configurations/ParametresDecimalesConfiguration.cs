using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class ParametresDecimalesConfiguration : IEntityTypeConfiguration<ParametresDecimales>
{
    public void Configure(EntityTypeBuilder<ParametresDecimales> builder)
    {
        builder.ToTable("ParametresDecimales");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("id");

        builder.Property(p => p.NombreDecimalesQuantite)
            .HasColumnName("nombre_decimales_quantite");

        builder.Property(p => p.NombreDecimalesPrix)
            .HasColumnName("nombre_decimales_prix");

        builder.Property(p => p.NombreDecimalesMontant)
            .HasColumnName("nombre_decimales_montant");
    }
}
