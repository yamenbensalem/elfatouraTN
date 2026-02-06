using GestCom.Application.Features.Configuration.DTOs;
using MediatR;

namespace GestCom.Application.Features.Configuration.ModesPaiement.Queries.GetAllModesPaiement;

/// <summary>
/// Query pour récupérer tous les modes de paiement
/// </summary>
public class GetAllModesPaiementQuery : IRequest<IEnumerable<ModePaiementDto>>
{
}
