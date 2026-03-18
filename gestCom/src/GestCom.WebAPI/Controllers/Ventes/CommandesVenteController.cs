using GestCom.Application.Features.Ventes.Commandes.Commands.CreateCommandeVente;
using GestCom.Application.Features.Ventes.Commandes.DTOs;
using GestCom.Shared.Common;
using GestCom.WebAPI.Controllers.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestCom.WebAPI.Controllers.Ventes;

/// <summary>
/// Contrôleur pour la gestion des commandes de vente
/// </summary>
[Authorize]
public class CommandesVenteController : BaseApiController
{
    /// <summary>
    /// Récupère la liste des commandes de vente
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CommandeVenteListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CommandeVenteListDto>>> GetAll(
        [FromQuery] string? codeClient,
        [FromQuery] string? statut,
        [FromQuery] DateTime? dateDebut,
        [FromQuery] DateTime? dateFin,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new Application.Features.Ventes.Commandes.Queries.GetAllCommandesVente.GetAllCommandesVenteQuery
        {
            CodeClient = codeClient,
            Statut = statut,
            DateDebut = dateDebut,
            DateFin = dateFin,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Récupère une commande par son numéro
    /// </summary>
    [HttpGet("{numero}", Name = "GetCommandeByNumero")]
    [ProducesResponseType(typeof(CommandeVenteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommandeVenteDto>> GetByNumero(string numero)
    {
        var query = new Application.Features.Ventes.Commandes.Queries.GetCommandeVenteByNumero.GetCommandeVenteByNumeroQuery(numero);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Crée une nouvelle commande de vente
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CommandeVenteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CommandeVenteDto>> Create([FromBody] CreateCommandeVenteCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetByNumero), new { numero = result.NumeroCommande }, result);
    }

    /// <summary>
    /// Met à jour une commande de vente
    /// </summary>
    [HttpPut("{numero}")]
    [ProducesResponseType(typeof(CommandeVenteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommandeVenteDto>> Update(string numero, [FromBody] GestCom.Application.Features.Ventes.Commandes.Commands.UpdateCommandeVente.UpdateCommandeVenteCommand command)
    {
        if (numero != command.NumeroCommande)
        {
            return BadRequest("Le numéro de la commande ne correspond pas à celui de l'URL.");
        }

        var result = await Mediator.Send(command);
        return Ok(result);
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Annuler(string numero)
    {
        // À implémenter
        return Ok(new { message = $"Commande {numero} annulée" });
    }
}