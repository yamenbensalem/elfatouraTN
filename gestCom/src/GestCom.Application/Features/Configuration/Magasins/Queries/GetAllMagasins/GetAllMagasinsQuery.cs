using GestCom.Application.Features.Configuration.DTOs;
using MediatR;

namespace GestCom.Application.Features.Configuration.Magasins.Queries.GetAllMagasins;

/// <summary>
/// Query pour récupérer tous les magasins/dépôts
/// </summary>
public class GetAllMagasinsQuery : IRequest<IEnumerable<MagasinProduitDto>>
{
}
