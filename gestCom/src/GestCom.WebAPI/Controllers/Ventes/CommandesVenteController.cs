using GestCom.Application.Features.Ventes.Commandes.Commands.CreateCommandeVente;
using GestCom.Application.Features.Ventes.Commandes.DTOs;
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
    [ProducesResponseType(typeof(IEnumerable<CommandeVenteListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CommandeVenteListDto>>> GetAll(
        [FromQuery] string? codeClient,
        [FromQuery] string? statut,
        [FromQuery] DateTime? dateDebut,
        [FromQuery] DateTime? dateFin)
    {
        // Pour l'instant, retourner une liste vide - à implémenter avec une Query dédiée
        return Ok(Array.Empty<CommandeVenteListDto>());
    }

    /// <summary>
    /// Récupère une commande par son numéro
    /// </summary>
    [HttpGet("{numero}", Name = "GetCommandeByNumero")]
    [ProducesResponseType(typeof(CommandeVenteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommandeVenteDto>> GetByNumero(string numero)
    {
        // À implémenter avec une Query dédiée
        return NotFound($"Commande '{numero}' non trouvée.");
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

public class UpdateStatutRequest
{
    public string Statut { get; set; } = string.Empty;
}
