using GestCom.Application.Features.Configuration.DTOs;
using GestCom.Application.Features.Configuration.Unites.Queries.GetAllUnites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestCom.WebAPI.Controllers.Configuration;

/// <summary>
/// Contrôleur pour la gestion des unités de mesure
/// </summary>
[Authorize]
public class UnitesController : BaseApiController
{
    /// <summary>
    /// Récupère toutes les unités de mesure
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UniteProduitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UniteProduitDto>>> GetAll()
    {
        var query = new GetAllUnitesQuery();
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Récupère une unité par son code
    /// </summary>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(UniteProduitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UniteProduitDto>> GetByCode(string code)
    {
        var query = new GetAllUnitesQuery();
        var result = await Mediator.Send(query);
        var unite = result.FirstOrDefault(u => u.CodeUnite == code);
        
        if (unite == null)
            return NotFound($"Unité '{code}' non trouvée.");
            
        return Ok(unite);
    }

    /// <summary>
    /// Crée une nouvelle unité
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(UniteProduitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UniteProduitDto>> Create([FromBody] CreateUniteProduitDto dto)
    {
        // À implémenter
        var result = new UniteProduitDto
        {
            CodeUnite = dto.CodeUnite,
            LibelleUnite = dto.LibelleUnite
        };
        return CreatedAtAction(nameof(GetByCode), new { code = result.CodeUnite }, result);
    }

    /// <summary>
    /// Supprime une unité
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

public class CreateUniteProduitDto
{
    public string CodeUnite { get; set; } = string.Empty;
    public string LibelleUnite { get; set; } = string.Empty;
}
