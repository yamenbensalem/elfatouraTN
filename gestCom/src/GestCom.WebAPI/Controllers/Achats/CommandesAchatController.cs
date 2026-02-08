using GestCom.Application.Features.Achats.CommandesAchat.Commands.CreateCommandeAchat;
using GestCom.Application.Features.Achats.CommandesAchat.DTOs;
using GestCom.Application.Common.Interfaces;
using GestCom.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestCom.WebAPI.Controllers.Configuration;

namespace GestCom.WebAPI.Controllers.Achats;

/// <summary>
/// Contrôleur pour la gestion des commandes d'achat
/// </summary>
[Authorize]
[Route("api/v1/commandes-achat")]
public class CommandesAchatController : BaseApiController
{
    private readonly IPdfService _pdfService;

    public CommandesAchatController(IPdfService pdfService)
    {
        _pdfService = pdfService;
    }

    /// <summary>
    /// Récupère la liste des commandes d'achat
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CommandeAchatListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CommandeAchatListDto>>> GetAll(
        [FromQuery] string? codeFournisseur,
        [FromQuery] string? statut,
        [FromQuery] DateTime? dateDebut,
        [FromQuery] DateTime? dateFin,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        // À implémenter avec une Query dédiée
        return Ok(new PagedResult<CommandeAchatListDto>(new List<CommandeAchatListDto>(), 0, pageNumber, pageSize));
    }

    /// <summary>
    /// Récupère une commande par son numéro
    /// </summary>
    [HttpGet("{numero}")]
    [ProducesResponseType(typeof(CommandeAchatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommandeAchatDto>> GetByNumero(string numero)
    {
        // À implémenter avec une Query dédiée
        return NotFound($"Commande d'achat '{numero}' non trouvée.");
    }

    /// <summary>
    /// Crée une nouvelle commande d'achat
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CommandeAchatDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CommandeAchatDto>> Create([FromBody] CreateCommandeAchatCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetByNumero), new { numero = result.NumeroCommande }, result);
    }

    /// <summary>
    /// Met à jour le statut d'une commande
    /// </summary>
    [HttpPatch("{numero}/statut")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateStatut(string numero, [FromBody] UpdateStatutRequest request)
    {
        // À implémenter
        return Ok(new { message = $"Statut de la commande {numero} mis à jour vers {request.Statut}" });
    }

    /// <summary>
    /// Annule une commande
    /// </summary>
    [HttpPost("{numero}/annuler")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Annuler(string numero)
    {
        // À implémenter
        return Ok(new { message = $"Commande d'achat {numero} annulée" });
    }

    /// <summary>
    /// Génère le PDF d'une commande d'achat
    /// </summary>
    [HttpGet("{numero}/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetPdf(string numero)
    {
        var pdfBytes = await _pdfService.GenerateCommandeAchatPdfAsync(numero);
        return File(pdfBytes, "application/pdf", $"CA_{numero}.pdf");
    }
}

