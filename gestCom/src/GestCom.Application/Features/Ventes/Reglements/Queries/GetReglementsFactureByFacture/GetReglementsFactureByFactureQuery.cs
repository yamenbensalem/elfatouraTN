using GestCom.Application.Features.Ventes.Reglements.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.Reglements.Queries.GetReglementsFactureByFacture;

public class GetReglementsFactureByFactureQuery : IRequest<IEnumerable<ReglementFactureDto>>
{
    public string NumeroFacture { get; set; } = string.Empty;
}