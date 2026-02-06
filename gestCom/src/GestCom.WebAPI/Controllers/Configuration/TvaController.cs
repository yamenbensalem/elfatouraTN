using GestCom.Application.Features.Configuration.DTOs;
using GestCom.Application.Features.Configuration.TVA.Commands.CreateTva;
using GestCom.Application.Features.Configuration.TVA.Queries.GetAllTva;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestCom.WebAPI.Controllers.Configuration;

/// <summary>
/// Contrôleur pour la gestion des taux de TVA
/// </summary>
[Authorize]
public class TvaController : BaseApiController
{
    /// <summary>
    /// Récupère tous les taux de TVA
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TvaProduitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TvaProduitDto>>> GetAll()
    {
        var query = new GetAllTvaQuery();
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Récupère un taux de TVA par son code
    /// </summary>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(TvaProduitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TvaProduitDto>> GetByCode(string code)
    {
        var query = new GetAllTvaQuery();
        var result = await Mediator.Send(query);
        var tva = result.FirstOrDefault(t => t.CodeTva == code);
        
        if (tva == null)
            return NotFound($"Taux TVA '{code}' non trouvé.");
            
        return Ok(tva);
    }

    /// <summary>
    /// Crée un nouveau taux de TVA
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(TvaProduitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TvaProduitDto>> Create([FromBody] CreateTvaCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetByCode), new { code = result.CodeTva }, result);
    }

    /// <summary>
    /// Met à jour un taux de TVA
    /// </summary>
    [HttpPut("{code}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(TvaProduitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TvaProduitDto>> Update(string code, [FromBody] UpdateTvaProduitDto dto)
    {
        // À implémenter
        return NotFound($"Taux TVA '{code}' non trouvé.");
    }

    /// <summary>
    /// Supprime un taux de TVA
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
