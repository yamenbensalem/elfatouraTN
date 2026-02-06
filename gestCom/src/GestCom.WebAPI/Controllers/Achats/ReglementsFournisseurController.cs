using GestCom.Application.Features.Achats.ReglementsFournisseur.Commands.CreateReglementFournisseur;
using GestCom.Application.Features.Achats.ReglementsFournisseur.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestCom.WebAPI.Controllers.Achats;

/// <summary>
/// Contrôleur pour la gestion des règlements fournisseurs
/// </summary>
[Authorize]
[Route("api/v1/reglements/fournisseurs")]
public class ReglementsFournisseurController : BaseApiController
{
    /// <summary>
    /// Récupère la liste des règlements fournisseurs
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ReglementFournisseurListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReglementFournisseurListDto>>> GetAll(
        [FromQuery] string? codeFournisseur,
        [FromQuery] string? numeroFacture,
        [FromQuery] DateTime? dateDebut,
        [FromQuery] DateTime? dateFin)
    {
        // À implémenter avec une Query dédiée
        return Ok(Array.Empty<ReglementFournisseurListDto>());
    }

    /// <summary>
    /// Récupère un règlement par son numéro
    /// </summary>
    [HttpGet("{numero}")]
    [ProducesResponseType(typeof(ReglementFournisseurDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReglementFournisseurDto>> GetByNumero(string numero)
    {
        // À implémenter avec une Query dédiée
        return NotFound($"Règlement '{numero}' non trouvé.");
    }

    /// <summary>
    /// Récupère les règlements d'une facture spécifique
    /// </summary>
    [HttpGet("facture/{numeroFacture}")]
    [ProducesResponseType(typeof(IEnumerable<ReglementFournisseurDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReglementFournisseurDto>>> GetByFacture(string numeroFacture)
    {
        // À implémenter avec une Query dédiée
        return Ok(Array.Empty<ReglementFournisseurDto>());
    }

    /// <summary>
    /// Crée un nouveau règlement fournisseur
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReglementFournisseurDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ReglementFournisseurDto>> Create([FromBody] CreateReglementFournisseurCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetByNumero), new { numero = result.NumeroReglement }, result);
    }

    /// <summary>
    /// Annule un règlement fournisseur
    /// </summary>
    [HttpPost("{numero}/annuler")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Annuler(string numero)
    {
        // À implémenter
        return Ok(new { message = $"Règlement {numero} annulé" });
    }
}
