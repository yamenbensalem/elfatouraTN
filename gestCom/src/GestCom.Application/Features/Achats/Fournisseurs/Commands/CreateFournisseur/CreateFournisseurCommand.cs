using GestCom.Application.Features.Achats.Fournisseurs.DTOs;
using MediatR;

namespace GestCom.Application.Features.Achats.Fournisseurs.Commands.CreateFournisseur;

public class CreateFournisseurCommand : IRequest<FournisseurDto>
{
    public string CodeFournisseur { get; set; } = string.Empty;
    public string? RaisonSociale { get; set; }
    public string? MatriculeFiscale { get; set; }
    public string? Adresse { get; set; }
    public string? CodePostal { get; set; }
    public string? Ville { get; set; }
    public string? Pays { get; set; }
    public string? Telephone { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? SiteWeb { get; set; }
    public string? Contact { get; set; }
    public string? Observations { get; set; }
    public decimal DelaiPaiement { get; set; }
    public decimal TauxRemise { get; set; }
}
