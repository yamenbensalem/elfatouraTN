using GestCom.Application.Features.Ventes.Reglements.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.Reglements.Queries.GetReglementFactureByNumero;

public class GetReglementFactureByNumeroQuery : IRequest<ReglementFactureDto?>
{
    public string NumeroReglement { get; set; } = string.Empty;
}