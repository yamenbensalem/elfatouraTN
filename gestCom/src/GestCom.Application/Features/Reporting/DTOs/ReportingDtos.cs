using System;
using System.Collections.Generic;

namespace GestCom.Application.Features.Reporting.DTOs;

/// <summary>
/// DTO pour le tableau de bord principal
/// </summary>
public class DashboardDto
{
    // Ventes
    public decimal ChiffreAffairesMois { get; set; }
    public decimal ChiffreAffairesAnnee { get; set; }
    public decimal ChiffreAffairesMoisPrecedent { get; set; }
    public decimal EvolutionCA { get; set; }
    
    // Créances
    public decimal TotalCreances { get; set; }
    public int FacturesImpayees { get; set; }
    
    // Achats
    public decimal TotalAchatsMois { get; set; }
    public decimal TotalDettes { get; set; }
    public int FacturesFournisseurImpayees { get; set; }
    
    // Stock
    public int ProduitsEnStock { get; set; }
    public int ProduitsStockFaible { get; set; }
    public decimal ValeurStock { get; set; }
    
    // Documents du jour
    public int DevisAujourdhui { get; set; }
    public int CommandesAujourdhui { get; set; }
    public int FacturesAujourdhui { get; set; }
    public int BonsLivraisonAujourdhui { get; set; }
    
    // Top clients
    public List<TopClientDto> TopClients { get; set; } = new();
    
    // Top produits
    public List<TopProduitDto> TopProduits { get; set; } = new();
    
    // Graphiques
    public List<ChiffreAffairesParMoisDto> CAParMois { get; set; } = new();
    public List<VentesParCategorieDto> VentesParCategorie { get; set; } = new();
}

/// <summary>
/// DTO pour les meilleurs clients
/// </summary>
public class TopClientDto
{
    public string CodeClient { get; set; } = string.Empty;
    public string? NomClient { get; set; }
    public decimal ChiffreAffaires { get; set; }
    public int NombreFactures { get; set; }
}

/// <summary>
/// DTO pour les meilleurs produits
/// </summary>
public class TopProduitDto
{
    public string CodeProduit { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public decimal QuantiteVendue { get; set; }
    public decimal ChiffreAffaires { get; set; }
}

/// <summary>
/// DTO pour le CA par mois
/// </summary>
public class ChiffreAffairesParMoisDto
{
    public int Annee { get; set; }
    public int Mois { get; set; }
    public string NomMois { get; set; } = string.Empty;
    public decimal MontantHT { get; set; }
    public decimal MontantTTC { get; set; }
}

/// <summary>
/// DTO pour les ventes par catégorie
/// </summary>
public class VentesParCategorieDto
{
    public string CodeCategorie { get; set; } = string.Empty;
    public string? LibelleCategorie { get; set; }
    public decimal MontantVentes { get; set; }
    public decimal Pourcentage { get; set; }
}

/// <summary>
/// DTO pour le rapport de stock
/// </summary>
public class RapportStockDto
{
    public int TotalProduits { get; set; }
    public int ProduitsEnStock { get; set; }
    public int ProduitsStockFaible { get; set; }
    public int ProduitsRupture { get; set; }
    public decimal ValeurStockTotal { get; set; }
    public List<ProduitStockDto> ProduitsAlertes { get; set; } = new();
    public List<StockParCategorieDto> StockParCategorie { get; set; } = new();
}

/// <summary>
/// DTO pour un produit avec alerte stock
/// </summary>
public class ProduitStockDto
{
    public string CodeProduit { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? LibelleCategorie { get; set; }
    public decimal Quantite { get; set; }
    public decimal StockMinimal { get; set; }
    public decimal Ecart { get; set; }
    public string Niveau { get; set; } = string.Empty; // "Rupture", "Critique", "Faible"
}

/// <summary>
/// DTO pour le stock par catégorie
/// </summary>
public class StockParCategorieDto
{
    public string CodeCategorie { get; set; } = string.Empty;
    public string? LibelleCategorie { get; set; }
    public int NombreProduits { get; set; }
    public decimal QuantiteTotale { get; set; }
    public decimal ValeurStock { get; set; }
}

/// <summary>
/// DTO pour le rapport des créances
/// </summary>
public class RapportCreancesDto
{
    public decimal TotalCreances { get; set; }
    public decimal CreancesNonEchues { get; set; }
    public decimal CreancesEchues { get; set; }
    public decimal CreancesEchues30Jours { get; set; }
    public decimal CreancesEchues60Jours { get; set; }
    public decimal CreancesEchues90Jours { get; set; }
    public decimal CreancesEchuesPlus90Jours { get; set; }
    public List<CreanceClientDto> ParClient { get; set; } = new();
}

/// <summary>
/// DTO pour les créances par client
/// </summary>
public class CreanceClientDto
{
    public string CodeClient { get; set; } = string.Empty;
    public string? NomClient { get; set; }
    public decimal TotalCreances { get; set; }
    public int NombreFacturesImpayees { get; set; }
    public DateTime? DatePlusAncienneFacture { get; set; }
    public int JoursRetard { get; set; }
}
