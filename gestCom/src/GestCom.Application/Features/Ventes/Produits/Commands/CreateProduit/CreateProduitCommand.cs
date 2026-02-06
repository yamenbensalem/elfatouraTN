using GestCom.Application.Features.Ventes.Produits.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.Produits.Commands.CreateProduit;

public class CreateProduitCommand : IRequest<ProduitDto>
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
