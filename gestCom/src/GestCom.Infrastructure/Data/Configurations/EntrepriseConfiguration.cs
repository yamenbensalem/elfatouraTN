using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class EntrepriseConfiguration : IEntityTypeConfiguration<Entreprise>
{
    public void Configure(EntityTypeBuilder<Entreprise> builder)
    {
        builder.ToTable("Entreprise");
        
        builder.HasKey(e => e.CodeEntreprise);
        
        builder.Property(e => e.CodeEntreprise)
            .HasMaxLength(50)
            .HasColumnName("codeentreprise");
            
        builder.Property(e => e.MatriculeFiscale)
            .HasMaxLength(50)
            .HasColumnName("matriculeFiscale_entreprise");
            
        builder.Property(e => e.RaisonSociale)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("raisonSociale_entreprise");
            
        builder.Property(e => e.NomCommercial)
            .HasMaxLength(200)
            .HasColumnName("nomCommercial_entreprise");
            
        builder.Property(e => e.TypePersonne)
            .HasMaxLength(50)
            .HasColumnName("typePersonne_entreprise");
            
        builder.Property(e => e.CompteBancaire)
            .HasMaxLength(50)
            .HasColumnName("comptebancaire_entreprise");
            
        builder.Property(e => e.RegistreCommerce)
            .HasMaxLength(50)
            .HasColumnName("registrecommerce_entreprise");
            
        builder.Property(e => e.CapitalSocial)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("capitalSocial_entreprise");
            
        builder.Property(e => e.Description)
            .HasColumnName("description_entreprise");
            
        builder.Property(e => e.Adresse)
            .HasMaxLength(500)
            .HasColumnName("adresse_entreprise");
            
        builder.Property(e => e.CodePostal)
            .HasMaxLength(20)
            .HasColumnName("codepostal_entreprise");
            
        builder.Property(e => e.Ville)
            .HasMaxLength(100)
            .HasColumnName("ville_entreprise");
            
        builder.Property(e => e.Pays)
            .HasMaxLength(100)
            .HasColumnName("pays_entreprise");
            
        builder.Property(e => e.TelFixe1)
            .HasMaxLength(20)
            .HasColumnName("telfixe1_entreprise");
            
        builder.Property(e => e.TelFixe2)
            .HasMaxLength(20)
            .HasColumnName("telfixe2_entreprise");
            
        builder.Property(e => e.TelMobile)
            .HasMaxLength(20)
            .HasColumnName("telmobile_entreprise");
            
        builder.Property(e => e.Fax)
            .HasMaxLength(20)
            .HasColumnName("fax_entreprise");
            
        builder.Property(e => e.Email)
            .HasMaxLength(100)
            .HasColumnName("email_entreprise");
            
        builder.Property(e => e.SiteWeb)
            .HasMaxLength(200)
            .HasColumnName("site_entreprise");
            
        builder.Property(e => e.Matricule)
            .HasMaxLength(50)
            .HasColumnName("matricule_entreprise");
            
        builder.Property(e => e.CodeTVA)
            .HasMaxLength(50)
            .HasColumnName("codeTVA_entreprise");
            
        builder.Property(e => e.CodeCategorie)
            .HasMaxLength(50)
            .HasColumnName("codecatego_entreprise");
            
        builder.Property(e => e.NumeroEtablissement)
            .HasMaxLength(50)
            .HasColumnName("numetab_entreprise");
            
        builder.Property(e => e.AssujittieTVA)
            .HasColumnName("assujittieTVA_entreprise");
            
        builder.Property(e => e.AssujittieFodec)
            .HasColumnName("assujittieFodec_entreprise");
            
        builder.Property(e => e.Exonore)
            .HasColumnName("exonore_entreprise");
            
        builder.Property(e => e.CodeDouane)
            .HasMaxLength(50)
            .HasColumnName("codedouane_entreprise");
            
        builder.Property(e => e.CodeDevise)
            .HasColumnName("code_devise");
            
        builder.Property(e => e.Logo)
            .HasColumnName("logo_entreprise");

        // Ignore alias properties
        builder.Ignore(e => e.MatriculeFiscal);
        builder.Ignore(e => e.Telephone);

        // Relations
        builder.HasOne(e => e.Devise)
            .WithMany()
            .HasForeignKey(e => e.CodeDevise)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
