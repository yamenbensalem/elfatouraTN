using GestCom.Application.Features.Configuration.DTOs;
using MediatR;

namespace GestCom.Application.Features.Configuration.Categories.Commands.CreateCategorie;

/// <summary>
/// Command pour créer une nouvelle catégorie de produit
/// </summary>
public class CreateCategorieCommand : IRequest<CategorieProduitDto>
{
    public string CodeCategorie { get; set; } = string.Empty;
    public string? LibelleCategorie { get; set; }
    public string? Description { get; set; }
}
