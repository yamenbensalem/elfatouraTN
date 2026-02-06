using GestCom.Application.Features.Achats.FacturesFournisseur.DTOs;
using MediatR;

namespace GestCom.Application.Features.Achats.FacturesFournisseur.Commands.CreateFactureFournisseur;

/// <summary>
/// Command pour créer une nouvelle facture fournisseur
/// </summary>
public class CreateFactureFournisseurCommand : IRequest<FactureFournisseurDto>
{
    public DateTime DateFacture { get; set; }
    public DateTime DateEcheance { get; set; }
    public string CodeFournisseur { get; set; } = string.Empty;
    public string? NumeroBonReception { get; set; }
    public string? NumeroFactureFournisseur { get; set; } // Numéro sur la facture du fournisseur
    public decimal TauxRemiseGlobale { get; set; }
    public string? Observation { get; set; }
    public List<CreateLigneFactureFournisseurDto> Lignes { get; set; } = new();
}

public class CreateLigneFactureFournisseurDto
{
    public string CodeProduit { get; set; } = string.Empty;
    public decimal Quantite { get; set; }
    public decimal PrixUnitaireHT { get; set; }
    public decimal TauxTVA { get; set; }
    public decimal TauxRemise { get; set; }
    public decimal TauxFodec { get; set; }
}
