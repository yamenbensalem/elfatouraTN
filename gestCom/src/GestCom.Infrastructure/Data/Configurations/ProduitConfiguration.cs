using GestCom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestCom.Infrastructure.Data.Configurations;

public class ProduitConfiguration : IEntityTypeConfiguration<Produit>
{
    public void Configure(EntityTypeBuilder<Produit> builder)
    {
        builder.ToTable("Produit");
        
        builder.HasKey(p => p.CodeProduit);
        
        builder.Property(p => p.CodeProduit)
            .HasMaxLength(50)
            .HasColumnName("code_produit");
            
        builder.Property(p => p.CodeEntreprise)
            .HasMaxLength(50)
            .HasColumnName("codeentreprise");
            
        builder.Property(p => p.Designation)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("designation_produit");
            
        builder.Property(p => p.PrixUnitaire)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("prixunitaire_produit");
            
        builder.Property(p => p.CodeDevise)
            .HasColumnName("code_devise");
            
        builder.Property(p => p.PrixAchatTTC)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("prixachatTTC_produit");
            
        builder.Property(p => p.TauxMarge)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("tauxmarge_produit");
            
        builder.Property(p => p.PrixVenteHT)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("prixventeHT_produit");
            
        builder.Property(p => p.Remise)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("remise_produit");
            
        builder.Property(p => p.PrixVenteTTC)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("prixventeTTC_produit");
            
        builder.Property(p => p.Fodec)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("fodec_produit");
            
        builder.Property(p => p.TauxTVA)
            .HasColumnType("decimal(18,3)");
            
        builder.Property(p => p.TauxFODEC)
            .HasColumnType("decimal(18,3)");
            
        builder.Property(p => p.Quantite)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("quantite_produit");
            
        builder.Property(p => p.StockMinimal)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("stockminimal_produit");
            
        builder.Property(p => p.RemiseMaximale)
            .HasColumnType("decimal(18,3)")
            .HasColumnName("remisemaximale_produit");
            
        builder.Property(p => p.Rayon)
            .HasMaxLength(50)
            .HasColumnName("rayon_produit");
            
        builder.Property(p => p.Etage)
            .HasMaxLength(50)
            .HasColumnName("etage_produit");
            
        builder.Property(p => p.CodeFournisseur)
            .HasMaxLength(50)
            .HasColumnName("code_fournisseur");
            
        builder.Property(p => p.CodeUniteProduit)
            .HasColumnName("code_uniteproduit");
            
        builder.Property(p => p.CodeTVAProduit)
            .HasColumnName("code_tvaproduit");
            
        builder.Property(p => p.CodeCategorieProduit)
            .HasColumnName("code_categorieproduit");
            
        builder.Property(p => p.CodeMagasinProduit)
            .HasColumnName("code_magasinproduit");
            
        builder.Property(p => p.CodeFabriquantProduit)
            .HasColumnName("code_fabriquantproduit");
            
        builder.Property(p => p.CodePaysProduit)
            .HasColumnName("code_paysproduit");
            
        builder.Property(p => p.CodeDouaneProduit)
            .HasColumnName("code_douaneproduit");

        // Relations
        builder.HasOne(p => p.Entreprise)
            .WithMany()
            .HasForeignKey(p => p.CodeEntreprise)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Fournisseur)
            .WithMany(f => f.Produits)
            .HasForeignKey(p => p.CodeFournisseur)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.UniteProduit)
            .WithMany()
            .HasForeignKey(p => p.CodeUniteProduit)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.TvaProduit)
            .WithMany()
            .HasForeignKey(p => p.CodeTVAProduit)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.CategorieProduit)
            .WithMany(c => c.Produits)
            .HasForeignKey(p => p.CodeCategorieProduit)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.MagasinProduit)
            .WithMany(m => m.Produits)
            .HasForeignKey(p => p.CodeMagasinProduit)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
