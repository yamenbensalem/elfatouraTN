using GestCom.Application.Features.Configuration.DTOs;
using MediatR;

namespace GestCom.Application.Features.Configuration.TVA.Commands.CreateTva;

/// <summary>
/// Command pour créer un taux de TVA
/// </summary>
public class CreateTvaCommand : IRequest<TvaProduitDto>
{
    public int CodeTVA { get; set; }
    public string Designation { get; set; } = string.Empty;
    public decimal Taux { get; set; }
    public bool ParDefaut { get; set; }
}
