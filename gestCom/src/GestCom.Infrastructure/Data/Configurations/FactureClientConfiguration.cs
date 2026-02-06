using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class FactureClientConfiguration : IEntityTypeConfiguration<FactureClient>
{
    public void Configure(EntityTypeBuilder<FactureClient> builder)
    {
        builder.ToTable("FactureClient");
        
        builder.HasKey(f => f.NumeroFacture);
        
        // Ignore alias properties
        builder.Ignore(f => f.MontantFODEC);
        builder.Ignore(f => f.LignesFacture);
        
        builder.Property(f => f.NumeroFacture)
            .HasMaxLength(50)
            .HasColumnName("numero_factureclient");
            
        builder.Property(f => f.CodeEntreprise)
            .HasMaxLength(50)
            .HasColumnName("codeentreprise");
            
        builder.Property(f => f.CodeClient)
            .HasMaxLength(50)
            .HasColumnName("codeclient_factureclient");
            
        builder.Property(f => f.DateFacture)
            .HasColumnName("date_factureclient");
            
        builder.Property(f => f.DateEcheance)
            .HasColumnName("dateecheance_factureclient");
            
        builder.Property(f => f.Remise)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("remise_factureclient");
            
        builder.Property(f => f.MontantHT)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("montantHT_factureclient");
            
        builder.Property(f => f.MontantTVA)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("montantTVA_factureclient");
            
        builder.Property(f => f.Timbre)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("timbre_factureclient");
            
        builder.Property(f => f.MontantTTC)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("montantTTC_factureclient");
            
        builder.Property(f => f.APayer)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("apayer_factureclient");
            
        builder.Property(f => f.MontantApresRAS)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("montantApresRAS_factureclient");
            
        builder.Property(f => f.MontantRestant)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("montantRestant_factureclient");
            
        builder.Property(f => f.Origine)
            .HasMaxLength(50)
            .HasColumnName("origine_factureclient");
            
        builder.Property(f => f.Statut)
            .HasMaxLength(50)
            .HasColumnName("statut_factureclient");
            
        builder.Property(f => f.ModePayement)
            .HasMaxLength(50)
            .HasColumnName("modepayement_factureclient");
            
        builder.Property(f => f.Avoir)
            .HasColumnName("avoir_factureclient");
            
        builder.Property(f => f.Notes)
            .HasColumnName("notes_factureclient");

        // Relations
        builder.HasOne(f => f.Entreprise)
            .WithMany()
            .HasForeignKey(f => f.CodeEntreprise)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Client)
            .WithMany(c => c.Factures)
            .HasForeignKey(f => f.CodeClient)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(f => f.Lignes)
            .WithOne(l => l.FactureClient)
            .HasForeignKey(l => l.NumeroFacture)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(f => f.Reglements)
            .WithOne(r => r.FactureClient)
            .HasForeignKey(r => r.NumeroFacture)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LigneFactureClientConfiguration : IEntityTypeConfiguration<LigneFactureClient>
{
    public void Configure(EntityTypeBuilder<LigneFactureClient> builder)
    {
        builder.ToTable("LigneFactureClient");
        
        builder.HasKey(l => l.Id);
        
        // Ignore alias properties
        builder.Ignore(l => l.TauxFODEC);
        builder.Ignore(l => l.MontantFODEC);
        
        builder.Property(l => l.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
            
        builder.Property(l => l.NumeroFacture)
            .HasMaxLength(50)
            .HasColumnName("numero_factureclient");
            
        builder.Property(l => l.CodeProduit)
            .HasMaxLength(50)
            .HasColumnName("code_produit");
            
        builder.Property(l => l.Designation)
            .HasMaxLength(200)
            .HasColumnName("designation");
            
        builder.Property(l => l.Quantite)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("quantite");
            
        builder.Property(l => l.PrixUnitaire)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("prixunitaire");
            
        builder.Property(l => l.Remise)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("remise");
            
        builder.Property(l => l.MontantHT)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("montantHT");
            
        builder.Property(l => l.TauxTVA)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("tauxTVA");
            
        builder.Property(l => l.MontantTVA)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("montantTVA");
            
        builder.Property(l => l.MontantTTC)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("montantTTC");

        // Relations
        builder.HasOne(l => l.Produit)
            .WithMany(p => p.LignesFactureClient)
            .HasForeignKey(l => l.CodeProduit)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
