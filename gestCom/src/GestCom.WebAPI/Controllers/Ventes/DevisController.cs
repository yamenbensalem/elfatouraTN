using GestCom.Application.Features.Ventes.Devis.Commands.ConvertDevisToCommande;
using GestCom.Application.Features.Ventes.Devis.Commands.CreateDevisClient;
using GestCom.Application.Features.Ventes.Devis.Commands.DeleteDevisClient;
using GestCom.Application.Features.Ventes.Devis.DTOs;
using GestCom.Application.Features.Ventes.Devis.Queries.GetAllDevisClient;
using GestCom.Application.Features.Ventes.Devis.Queries.GetDevisByNumero;
using GestCom.Application.Features.Ventes.Commandes.DTOs;
using GestCom.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestCom.WebAPI.Controllers.Ventes;

/// <summary>
/// Contrôleur pour la gestion des devis clients
/// </summary>
[Authorize]
public class DevisController : BaseApiController
{
    private readonly IPdfService _pdfService;

    public DevisController(IPdfService pdfService)
    {
        _pdfService = pdfService;
    }

    /// <summary>
    /// Récupère la liste des devis clients avec filtres optionnels
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DevisClientListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DevisClientListDto>>> GetAll([FromQuery] GetAllDevisClientQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Récupère un devis par son numéro
    /// </summary>
    [HttpGet("{numero}")]
    [ProducesResponseType(typeof(DevisClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DevisClientDto>> GetByNumero(string numero)
    {
        var query = new GetDevisByNumeroQuery { NumeroDevis = numero };
        var result = await Mediator.Send(query);
        
        if (result == null)
            return NotFound($"Devis '{numero}' non trouvé.");
            
        return Ok(result);
    }

    /// <summary>
    /// Crée un nouveau devis client
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DevisClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DevisClientDto>> Create([FromBody] CreateDevisClientCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetByNumero), new { numero = result.NumeroDevis }, result);
    }

    /// <summary>
    /// Convertit un devis en commande
    /// </summary>
    [HttpPost("{numero}/convertir")]
    [ProducesResponseType(typeof(CommandeVenteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommandeVenteDto>> ConvertToCommande(string numero, [FromBody] ConvertDevisToCommandeCommand command)
    {
        if (numero != command.NumeroDevis)
            return BadRequest("Le numéro du devis dans l'URL ne correspond pas au numéro dans le body.");

        var result = await Mediator.Send(command);
        return CreatedAtRoute("GetCommandeByNumero", new { numero = result.NumeroCommande }, result);
    }

    /// <summary>
    /// Supprime un devis (uniquement si non converti)
    /// </summary>
    [HttpDelete("{numero}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(string numero)
    {
        var command = new DeleteDevisClientCommand { NumeroDevis = numero };
        await Mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Génère le PDF d'un devis
    /// </summary>
    [HttpGet("{numero}/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetPdf(string numero)
    {
        var pdfBytes = await _pdfService.GenerateDevisClientPdfAsync(numero);
        return File(pdfBytes, "application/pdf", $"Devis_{numero}.pdf");
    }
}
