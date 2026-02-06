using GestCom.Application.Features.Achats.BonsReception.Commands.CreateBonReception;
using GestCom.Application.Features.Achats.BonsReception.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestCom.WebAPI.Controllers.Achats;

/// <summary>
/// Contrôleur pour la gestion des bons de réception
/// </summary>
[Authorize]
[Route("api/v1/bons-reception")]
public class BonsReceptionController : BaseApiController
{
    /// <summary>
    /// Récupère la liste des bons de réception
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BonReceptionListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BonReceptionListDto>>> GetAll(
        [FromQuery] string? codeFournisseur,
        [FromQuery] DateTime? dateDebut,
        [FromQuery] DateTime? dateFin)
    {
        // À implémenter avec une Query dédiée
        return Ok(Array.Empty<BonReceptionListDto>());
    }

    /// <summary>
    /// Récupère un bon de réception par son numéro
    /// </summary>
    [HttpGet("{numero}")]
    [ProducesResponseType(typeof(BonReceptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BonReceptionDto>> GetByNumero(string numero)
    {
        // À implémenter avec une Query dédiée
        return NotFound($"Bon de réception '{numero}' non trouvé.");
    }

    /// <summary>
    /// Crée un nouveau bon de réception
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BonReceptionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BonReceptionDto>> Create([FromBody] CreateBonReceptionCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetByNumero), new { numero = result.NumeroBonReception }, result);
    }

    /// <summary>
    /// Supprime un bon de réception
    /// </summary>
    [HttpDelete("{numero}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(string numero, [FromQuery] bool restaurerStock = true)
    {
        // À implémenter - doit décrémenter le stock si restaurerStock = true
        return NoContent();
    }
}
