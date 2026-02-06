using GestCom.Application.Features.Achats.FacturesFournisseur.Commands.CreateFactureFournisseur;
using GestCom.Application.Features.Achats.FacturesFournisseur.DTOs;
using GestCom.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestCom.WebAPI.Controllers.Achats;

/// <summary>
/// Contrôleur pour la gestion des factures fournisseurs
/// </summary>
[Authorize]
[Route("api/v1/factures/fournisseurs")]
public class FacturesFournisseurController : BaseApiController
{
    /// <summary>
    /// Récupère la liste des factures fournisseurs
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<FactureFournisseurListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<FactureFournisseurListDto>>> GetAll(
        [FromQuery] string? codeFournisseur,
        [FromQuery] string? statut,
        [FromQuery] DateTime? dateDebut,
        [FromQuery] DateTime? dateFin,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        // À implémenter avec une Query dédiée
        return Ok(new PagedResult<FactureFournisseurListDto>(new List<FactureFournisseurListDto>(), 0, pageNumber, pageSize));
    }

    /// <summary>
    /// Récupère une facture par son numéro
    /// </summary>
    [HttpGet("{numero}")]
    [ProducesResponseType(typeof(FactureFournisseurDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FactureFournisseurDto>> GetByNumero(string numero)
    {
        // À implémenter avec une Query dédiée
        return NotFound($"Facture fournisseur '{numero}' non trouvée.");
    }

    /// <summary>
    /// Récupère les factures échues
    /// </summary>
    [HttpGet("echues")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetFacturesEchues([FromQuery] DateTime? dateReference)
    {
        // À implémenter avec une Query dédiée
        return Ok(Array.Empty<object>());
    }

    /// <summary>
    /// Crée une nouvelle facture fournisseur
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(FactureFournisseurDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FactureFournisseurDto>> Create([FromBody] CreateFactureFournisseurCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetByNumero), new { numero = result.NumeroFacture }, result);
    }

    /// <summary>
    /// Supprime une facture fournisseur (si pas de règlements)
    /// </summary>
    [HttpDelete("{numero}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(string numero)
    {
        // À implémenter
        return NoContent();
    }
}
