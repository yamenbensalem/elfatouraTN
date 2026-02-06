using GestCom.Application.Features.Configuration.Categories.Commands.CreateCategorie;
using GestCom.Application.Features.Configuration.Categories.Queries.GetAllCategories;
using GestCom.Application.Features.Configuration.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestCom.WebAPI.Controllers.Configuration;

/// <summary>
/// Contrôleur pour la gestion des catégories de produits
/// </summary>
[Authorize]
public class CategoriesController : BaseApiController
{
    /// <summary>
    /// Récupère toutes les catégories
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategorieProduitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategorieProduitDto>>> GetAll([FromQuery] string? recherche)
    {
        var query = new GetAllCategoriesQuery { Recherche = recherche };
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Récupère une catégorie par son code
    /// </summary>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(CategorieProduitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategorieProduitDto>> GetByCode(string code)
    {
        var query = new GetAllCategoriesQuery();
        var result = await Mediator.Send(query);
        var categorie = result.FirstOrDefault(c => c.CodeCategorie == code);
        
        if (categorie == null)
            return NotFound($"Catégorie '{code}' non trouvée.");
            
        return Ok(categorie);
    }

    /// <summary>
    /// Crée une nouvelle catégorie
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(CategorieProduitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategorieProduitDto>> Create([FromBody] CreateCategorieCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetByCode), new { code = result.CodeCategorie }, result);
    }

    /// <summary>
    /// Met à jour une catégorie
    /// </summary>
    [HttpPut("{code}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(CategorieProduitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategorieProduitDto>> Update(string code, [FromBody] UpdateCategorieProduitDto dto)
    {
        // À implémenter
        return NotFound($"Catégorie '{code}' non trouvée.");
    }

    /// <summary>
    /// Supprime une catégorie
    /// </summary>
    [HttpDelete("{code}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete(string code)
    {
        // À implémenter - vérifier qu'il n'y a pas de produits liés
        return NoContent();
    }
}
