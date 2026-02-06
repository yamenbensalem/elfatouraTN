using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("id");

        builder.Property(r => r.Token)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnName("token");

        builder.Property(r => r.UserId)
            .IsRequired()
            .HasMaxLength(450)
            .HasColumnName("user_id");

        builder.Property(r => r.Created)
            .HasColumnName("created");

        builder.Property(r => r.Expires)
            .HasColumnName("expires");

        builder.Property(r => r.Revoked)
            .HasColumnName("revoked");

        builder.Property(r => r.ReplacedByToken)
            .HasMaxLength(500)
            .HasColumnName("replaced_by_token");

        builder.Property(r => r.ReasonRevoked)
            .HasMaxLength(500)
            .HasColumnName("reason_revoked");

        // Ignore computed properties
        builder.Ignore(r => r.IsExpired);
        builder.Ignore(r => r.IsRevoked);
        builder.Ignore(r => r.IsActive);

        // Relationships
        builder.HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.Token);
        builder.HasIndex(r => r.UserId);
    }
}
