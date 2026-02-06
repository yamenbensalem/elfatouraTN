using System;

namespace GestCom.Application.Features.Ventes.Produits.DTOs;

/// <summary>
/// DTO complet pour un produit
/// </summary>
public class ProduitDto
{
    public string CodeProduit { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? CodeBarre { get; set; }
    public string? Reference { get; set; }
    
    // Prix
    public decimal PrixAchatTTC { get; set; }
    public decimal TauxMarge { get; set; }
    public decimal PrixVenteHT { get; set; }
    public decimal PrixVenteTTC { get; set; }
    
    // Stock
    public decimal Quantite { get; set; }
    public decimal StockMinimal { get; set; }
    public bool IsStockFaible => Quantite <= StockMinimal;
    
    // TVA & FODEC
    public decimal TauxTVA { get; set; }
    public decimal TauxFODEC { get; set; }
    public bool Fodec { get; set; }
    
    // Relations
    public string? CodeFournisseur { get; set; }
    public string? NomFournisseur { get; set; }
    public string? CodeUnite { get; set; }
    public string? LibelleUnite { get; set; }
    public string? CodeCategorie { get; set; }
    public string? LibelleCategorie { get; set; }
    public string? CodeMagasin { get; set; }
    public string? LibelleMagasin { get; set; }
    public string? CodeTVA { get; set; }
    
    // Audit
    public DateTime DateCreation { get; set; }
    public DateTime? DateModification { get; set; }
}

/// <summary>
/// DTO simplifié pour les listes de produits
/// </summary>
public class ProduitListDto
{
    public string CodeProduit { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? CodeBarre { get; set; }
    public decimal PrixVenteTTC { get; set; }
    public decimal Quantite { get; set; }
    public decimal StockMinimal { get; set; }
    public bool IsStockFaible => Quantite <= StockMinimal;
    public string? LibelleCategorie { get; set; }
    public string? NomFournisseur { get; set; }
}

/// <summary>
/// DTO pour la création d'un produit
/// </summary>
public class CreateProduitDto
{
    public string CodeProduit { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? CodeBarre { get; set; }
    public string? Reference { get; set; }
    
    public decimal PrixAchatTTC { get; set; }
    public decimal TauxMarge { get; set; }
    public decimal PrixVenteHT { get; set; }
    public decimal PrixVenteTTC { get; set; }
    
    public decimal Quantite { get; set; }
    public decimal StockMinimal { get; set; }
    
    public decimal TauxTVA { get; set; }
    public decimal TauxFODEC { get; set; }
    public bool Fodec { get; set; }
    
    public string? CodeFournisseur { get; set; }
    public string? CodeUnite { get; set; }
    public string? CodeCategorie { get; set; }
    public string? CodeMagasin { get; set; }
    public string? CodeTVA { get; set; }
}

/// <summary>
/// DTO pour la mise à jour d'un produit
/// </summary>
public class UpdateProduitDto
{
    public string CodeProduit { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? CodeBarre { get; set; }
    public string? Reference { get; set; }
    
    public decimal PrixAchatTTC { get; set; }
    public decimal TauxMarge { get; set; }
    public decimal PrixVenteHT { get; set; }
    public decimal PrixVenteTTC { get; set; }
    
    public decimal Quantite { get; set; }
    public decimal StockMinimal { get; set; }
    
    public decimal TauxTVA { get; set; }
    public decimal TauxFODEC { get; set; }
    public bool Fodec { get; set; }
    
    public string? CodeFournisseur { get; set; }
    public string? CodeUnite { get; set; }
    public string? CodeCategorie { get; set; }
    public string? CodeMagasin { get; set; }
    public string? CodeTVA { get; set; }
}

/// <summary>
/// DTO pour recherche de produits
/// </summary>
public class SearchProduitDto
{
    public string CodeProduit { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? CodeBarre { get; set; }
    public decimal PrixVenteTTC { get; set; }
    public decimal Quantite { get; set; }
}
