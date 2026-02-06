using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Client");
        
        builder.HasKey(c => c.CodeClient);
        
        builder.Property(c => c.CodeClient)
            .HasMaxLength(50)
            .HasColumnName("code_client");
            
        builder.Property(c => c.CodeEntreprise)
            .HasMaxLength(50)
            .HasColumnName("codeentreprise");
            
        builder.Property(c => c.MatriculeFiscale)
            .HasMaxLength(50)
            .HasColumnName("matriculefiscale_client");
            
        builder.Property(c => c.Nom)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("nom_client");
            
        builder.Property(c => c.TypePersonne)
            .HasMaxLength(50)
            .HasColumnName("typepersonne_client");
            
        builder.Property(c => c.TypeEntreprise)
            .HasMaxLength(50)
            .HasColumnName("typeentreprise_client");
            
        builder.Property(c => c.RIB)
            .HasMaxLength(50)
            .HasColumnName("rib_client");
            
        builder.Property(c => c.Adresse)
            .HasMaxLength(500)
            .HasColumnName("adresse_client");
            
        builder.Property(c => c.CodePostal)
            .HasMaxLength(20)
            .HasColumnName("codepostal_client");
            
        builder.Property(c => c.Ville)
            .HasMaxLength(100)
            .HasColumnName("ville_client");
            
        builder.Property(c => c.Pays)
            .HasMaxLength(100)
            .HasColumnName("pays_client");
            
        builder.Property(c => c.Tel)
            .HasMaxLength(20)
            .HasColumnName("tel_client");
            
        builder.Property(c => c.TelMobile)
            .HasMaxLength(20)
            .HasColumnName("telmobile_client");
            
        builder.Property(c => c.Fax)
            .HasMaxLength(20)
            .HasColumnName("fax_client");
            
        builder.Property(c => c.Email)
            .HasMaxLength(100)
            .HasColumnName("email_client");
            
        builder.Property(c => c.SiteWeb)
            .HasMaxLength(200)
            .HasColumnName("site_client");
            
        builder.Property(c => c.Etat)
            .HasMaxLength(50)
            .HasColumnName("etat_client");
            
        builder.Property(c => c.NombreTransactions)
            .HasColumnName("nbtransactions_client");
            
        builder.Property(c => c.Note)
            .HasColumnName("note_client");
            
        builder.Property(c => c.Etranger)
            .HasColumnName("etranger_client");
            
        builder.Property(c => c.Exonore)
            .HasColumnName("exonore_client");
            
        builder.Property(c => c.MaxCredit)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("maxcredit_client");
            
        builder.Property(c => c.CodeDevise)
            .HasColumnName("code_devise");
            
        builder.Property(c => c.Responsable)
            .HasMaxLength(100)
            .HasColumnName("responsable_client");

        // Ignore alias properties
        builder.Ignore(c => c.RaisonSociale);
        builder.Ignore(c => c.NomClient);
        builder.Ignore(c => c.Telephone);

        // Relations
        builder.HasOne(c => c.Entreprise)
            .WithMany()
            .HasForeignKey(c => c.CodeEntreprise)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Devise)
            .WithMany()
            .HasForeignKey(c => c.CodeDevise)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Devis)
            .WithOne(d => d.Client)
            .HasForeignKey(d => d.CodeClient)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Commandes)
            .WithOne(cmd => cmd.Client)
            .HasForeignKey(cmd => cmd.CodeClient)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.BonsLivraison)
            .WithOne(bl => bl.Client)
            .HasForeignKey(bl => bl.CodeClient)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Factures)
            .WithOne(f => f.Client)
            .HasForeignKey(f => f.CodeClient)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Reglements)
            .WithOne(r => r.Client)
            .HasForeignKey(r => r.CodeClient)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
