using GestCom.Application.Features.Configuration.DTOs;
using MediatR;

namespace GestCom.Application.Features.Configuration.Unites.Queries.GetAllUnites;

/// <summary>
/// Query pour récupérer toutes les unités de mesure
/// </summary>
public class GetAllUnitesQuery : IRequest<IEnumerable<UniteProduitDto>>
{
}
