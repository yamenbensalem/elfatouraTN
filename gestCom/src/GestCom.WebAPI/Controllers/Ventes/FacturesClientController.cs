using GestCom.Application.Features.Ventes.Factures.Commands.CreateFactureClient;
using GestCom.Application.Features.Ventes.Factures.Commands.DeleteFactureClient;
using GestCom.Application.Features.Ventes.Factures.DTOs;
using GestCom.Application.Features.Ventes.Factures.Queries.GetAllFacturesClient;
using GestCom.Application.Features.Ventes.Factures.Queries.GetChiffreAffaires;
using GestCom.Application.Features.Ventes.Factures.Queries.GetFactureClientByNumero;
using GestCom.Application.Features.Ventes.Factures.Queries.GetFacturesEchues;
using GestCom.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestCom.WebAPI.Controllers.Ventes;

/// <summary>
/// Contrôleur pour la gestion des factures clients
/// </summary>
[Authorize]
[Route("api/v1/factures/clients")]
public class FacturesClientController : BaseApiController
{
    private readonly IPdfService _pdfService;

    public FacturesClientController(IPdfService pdfService)
    {
        _pdfService = pdfService;
    }

    /// <summary>
    /// Récupère la liste des factures clients avec filtres optionnels
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FactureClientListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FactureClientListDto>>> GetAll([FromQuery] GetAllFacturesClientQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Récupère une facture par son numéro
    /// </summary>
    [HttpGet("{numero}")]
    [ProducesResponseType(typeof(FactureClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FactureClientDto>> GetByNumero(string numero)
    {
        var query = new GetFactureClientByNumeroQuery { NumeroFacture = numero };
        var result = await Mediator.Send(query);
        
        if (result == null)
            return NotFound($"Facture '{numero}' non trouvée.");
            
        return Ok(result);
    }

    /// <summary>
    /// Récupère les factures échues (impayées passé date d'échéance)
    /// </summary>
    [HttpGet("echues")]
    [ProducesResponseType(typeof(IEnumerable<FactureEchueDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FactureEchueDto>>> GetFacturesEchues([FromQuery] GetFacturesEchuesQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Récupère le chiffre d'affaires pour une période donnée
    /// </summary>
    [HttpGet("chiffre-affaires")]
    [ProducesResponseType(typeof(ChiffreAffairesDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ChiffreAffairesDto>> GetChiffreAffaires([FromQuery] GetChiffreAffairesQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Crée une nouvelle facture client
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(FactureClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<FactureClientDto>> Create([FromBody] CreateFactureClientCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetByNumero), new { numero = result.NumeroFacture }, result);
    }

    /// <summary>
    /// Supprime une facture (uniquement si pas de règlements)
    /// </summary>
    [HttpDelete("{numero}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(string numero, [FromQuery] bool restaurerStock = true)
    {
        var command = new DeleteFactureClientCommand 
        { 
            NumeroFacture = numero,
            RestaurerStock = restaurerStock
        };
        await Mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Génère le PDF d'une facture
    /// </summary>
    [HttpGet("{numero}/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetPdf(string numero)
    {
        var pdfBytes = await _pdfService.GenerateFactureClientPdfAsync(numero);
        return File(pdfBytes, "application/pdf", $"Facture_{numero}.pdf");
    }
}
