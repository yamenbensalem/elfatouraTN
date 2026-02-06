using GestCom.Application.Features.Configuration.DTOs;
using MediatR;

namespace GestCom.Application.Features.Configuration.TVA.Queries.GetAllTva;

/// <summary>
/// Query pour récupérer tous les taux de TVA
/// </summary>
public class GetAllTvaQuery : IRequest<IEnumerable<TvaProduitDto>>
{
}
