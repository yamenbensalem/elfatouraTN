using GestCom.Application.Features.Ventes.Devis.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.Devis.Queries.GetDevisByNumero;

/// <summary>
/// Requête pour obtenir un devis par son numéro
/// </summary>
public class GetDevisByNumeroQuery : IRequest<DevisClientDto?>
{
    public string NumeroDevis { get; set; } = string.Empty;
}
