using GestCom.Application.Features.Ventes.Factures.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.Factures.Queries.GetFactureClientByNumero;

public class GetFactureClientByNumeroQuery : IRequest<FactureClientDto?>
{
    public string NumeroFacture { get; set; } = string.Empty;
}
