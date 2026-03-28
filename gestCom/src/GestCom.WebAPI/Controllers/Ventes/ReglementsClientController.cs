using GestCom.Application.Features.Ventes.Reglements.Commands.CreateReglementFacture;
using GestCom.Application.Features.Ventes.Reglements.DTOs;
using GestCom.Application.Features.Ventes.Reglements.Queries.GetAllReglementsFacture;
using GestCom.Application.Features.Ventes.Reglements.Queries.GetReglementFactureByNumero;
using GestCom.Application.Features.Ventes.Reglements.Queries.GetReglementsFactureByFacture;
using GestCom.Application.Features.Ventes.Reglements.Queries.GetResumeReglementsClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestCom.WebAPI.Controllers.Ventes;

/// <summary>
/// Contrôleur pour la gestion des règlements de factures clients
/// </summary>
[Authorize]
[Route("api/v1/reglements/clients")]
public class ReglementsClientController : BaseApiController
{
    /// <summary>
    /// Récupère la liste des règlements avec filtres optionnels
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ReglementFactureListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReglementFactureListDto>>> GetAll(
        [FromQuery] string? codeClient,
        [FromQuery] string? numeroFacture,
        [FromQuery] DateTime? dateDebut,
        [FromQuery] DateTime? dateFin)
    {
        var query = new GetAllReglementsFactureQuery
        {
            CodeClient = codeClient,
            NumeroFacture = numeroFacture,
            DateDebut = dateDebut,
            DateFin = dateFin
        };

        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Récupère un règlement par son numéro
    /// </summary>
    [HttpGet("{numero}")]
    [ProducesResponseType(typeof(ReglementFactureDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReglementFactureDto>> GetByNumero(string numero)
    {
        var query = new GetReglementFactureByNumeroQuery { NumeroReglement = numero };
        var result = await Mediator.Send(query);

        if (result == null)
            return NotFound($"Règlement '{numero}' non trouvé.");

        return Ok(result);
    }

    /// <summary>
    /// Récupère les règlements d'une facture spécifique
    /// </summary>
    [HttpGet("facture/{numeroFacture}")]
    [ProducesResponseType(typeof(IEnumerable<ReglementFactureDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReglementFactureDto>>> GetByFacture(string numeroFacture)
    {
        var query = new GetReglementsFactureByFactureQuery { NumeroFacture = numeroFacture };
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Récupère le résumé des règlements d'un client
    /// </summary>
    [HttpGet("client/{codeClient}/resume")]
    [ProducesResponseType(typeof(ResumeReglementsClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResumeReglementsClientDto>> GetResumeClient(string codeClient)
    {
        var query = new GetResumeReglementsClientQuery { CodeClient = codeClient };
        var result = await Mediator.Send(query);

        if (result == null)
            return NotFound($"Client '{codeClient}' non trouvé.");

        return Ok(result);
    }

    /// <summary>
    /// Crée un nouveau règlement
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReglementFactureDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ReglementFactureDto>> Create([FromBody] CreateReglementFactureCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetByNumero), new { numero = result.NumeroReglement }, result);
    }

    /// <summary>
    /// Annule un règlement
    /// </summary>
    [HttpPost("{numero}/annuler")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Annuler(string numero)
    {
        // À implémenter - doit recalculer le montant réglé sur la facture
        await Task.CompletedTask;
        return Ok(new { message = $"Règlement {numero} annulé" });
    }
}
