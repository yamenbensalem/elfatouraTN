using GestCom.Application.Features.Configuration.DTOs;
using MediatR;

namespace GestCom.Application.Features.Configuration.Entreprise.Queries.GetEntreprise;

/// <summary>
/// Query pour récupérer les informations de l'entreprise
/// </summary>
public class GetEntrepriseQuery : IRequest<EntrepriseDto?>
{
    /// <summary>
    /// Code de l'entreprise (optionnel pour le mode multi-tenant)
    /// </summary>
    public string? CodeEntreprise { get; set; }
}
