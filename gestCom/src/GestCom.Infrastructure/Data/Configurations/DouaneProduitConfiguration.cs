using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class DouaneProduitConfiguration : IEntityTypeConfiguration<DouaneProduit>
{
    public void Configure(EntityTypeBuilder<DouaneProduit> builder)
    {
        builder.ToTable("DouaneProduit");

        builder.HasKey(d => d.CodeDouane);

        builder.Property(d => d.CodeDouane)
            .ValueGeneratedOnAdd()
            .HasColumnName("code_douane");

        builder.Property(d => d.Designation)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("designation");

        builder.Property(d => d.NumeroHS)
            .HasMaxLength(50)
            .HasColumnName("numero_hs");

        builder.Property(d => d.Description)
            .HasMaxLength(500)
            .HasColumnName("description");
    }
}
