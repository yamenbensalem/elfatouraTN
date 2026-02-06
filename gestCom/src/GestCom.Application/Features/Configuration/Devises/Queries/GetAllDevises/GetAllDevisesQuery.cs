using GestCom.Application.Features.Configuration.DTOs;
using MediatR;

namespace GestCom.Application.Features.Configuration.Devises.Queries.GetAllDevises;

/// <summary>
/// Query pour récupérer toutes les devises
/// </summary>
public class GetAllDevisesQuery : IRequest<IEnumerable<DeviseDto>>
{
}
