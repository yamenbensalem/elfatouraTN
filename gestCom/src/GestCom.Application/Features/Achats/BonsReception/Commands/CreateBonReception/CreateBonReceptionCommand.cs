using GestCom.Application.Features.Achats.BonsReception.DTOs;
using MediatR;

namespace GestCom.Application.Features.Achats.BonsReception.Commands.CreateBonReception;

/// <summary>
/// Command pour créer un nouveau bon de réception
/// </summary>
public class CreateBonReceptionCommand : IRequest<BonReceptionDto>
{
    public string CodeEntreprise { get; set; } = string.Empty;
    public DateTime DateReception { get; set; }
    public string CodeFournisseur { get; set; } = string.Empty;
    public string? NumeroCommande { get; set; }
    public string? Notes { get; set; }
    public List<CreateLigneBonReceptionDto> Lignes { get; set; } = new();
}

public class CreateLigneBonReceptionDto
{
    public string CodeProduit { get; set; } = string.Empty;
    public decimal Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal TauxTVA { get; set; }
    public decimal Remise { get; set; }
}
