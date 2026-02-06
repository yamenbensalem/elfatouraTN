using GestCom.Application.Features.Configuration.DTOs;
using MediatR;

namespace GestCom.Application.Features.Configuration.Entreprise.Commands.UpdateEntreprise;

/// <summary>
/// Command pour mettre à jour les informations de l'entreprise
/// </summary>
public class UpdateEntrepriseCommand : IRequest<EntrepriseDto>
{
    public string CodeEntreprise { get; set; } = string.Empty;
    public string? RaisonSociale { get; set; }
    public string? MatriculeFiscal { get; set; }
    public string? Adresse { get; set; }
    public string? CodePostal { get; set; }
    public string? Ville { get; set; }
    public string? Telephone { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? SiteWeb { get; set; }
    public string? RIB { get; set; }
    public string? NomBanque { get; set; }
    public string? CodeDevise { get; set; }
    public string? Logo { get; set; }
}
