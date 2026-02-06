using GestCom.Application.Features.Ventes.Clients.Commands.CreateClient;
using GestCom.Application.Features.Ventes.Clients.Commands.DeleteClient;
using GestCom.Application.Features.Ventes.Clients.Commands.UpdateClient;
using GestCom.Application.Features.Ventes.Clients.DTOs;
using GestCom.Application.Features.Ventes.Clients.Queries.GetAllClients;
using GestCom.Application.Features.Ventes.Clients.Queries.GetClientBalance;
using GestCom.Application.Features.Ventes.Clients.Queries.GetClientByCode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestCom.WebAPI.Controllers.Ventes;

/// <summary>
/// Contrôleur pour la gestion des clients
/// </summary>
[Authorize]
public class ClientsController : BaseApiController
{
    /// <summary>
    /// Récupère la liste de tous les clients avec filtres optionnels
    /// </summary>
    /// <param name="query">Paramètres de filtrage et pagination</param>
    /// <returns>Liste des clients</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClientListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClientListDto>>> GetAll([FromQuery] GetAllClientsQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Récupère un client par son code
    /// </summary>
    /// <param name="code">Code du client</param>
    /// <returns>Détails du client</returns>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDto>> GetByCode(string code)
    {
        var query = new GetClientByCodeQuery { CodeClient = code };
        var result = await Mediator.Send(query);
        
        if (result == null)
            return NotFound($"Client avec le code '{code}' non trouvé.");
            
        return Ok(result);
    }

    /// <summary>
    /// Récupère le solde et l'historique de paiement d'un client
    /// </summary>
    /// <param name="code">Code du client</param>
    /// <returns>Solde et factures impayées</returns>
    [HttpGet("{code}/balance")]
    [ProducesResponseType(typeof(ClientBalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientBalanceDto>> GetBalance(string code)
    {
        var query = new GetClientBalanceQuery { CodeClient = code };
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Crée un nouveau client
    /// </summary>
    /// <param name="command">Données du client à créer</param>
    /// <returns>Client créé</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClientDto>> Create([FromBody] CreateClientCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetByCode), new { code = result.CodeClient }, result);
    }

    /// <summary>
    /// Met à jour un client existant
    /// </summary>
    /// <param name="code">Code du client</param>
    /// <param name="command">Données de mise à jour</param>
    /// <returns>Client mis à jour</returns>
    [HttpPut("{code}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDto>> Update(string code, [FromBody] UpdateClientCommand command)
    {
        if (code != command.CodeClient)
            return BadRequest("Le code du client dans l'URL ne correspond pas au code dans le body.");

        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Supprime un client
    /// </summary>
    /// <param name="code">Code du client à supprimer</param>
    /// <returns>NoContent si succès</returns>
    [HttpDelete("{code}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete(string code)
    {
        var command = new DeleteClientCommand { CodeClient = code };
        await Mediator.Send(command);
        return NoContent();
    }
}
