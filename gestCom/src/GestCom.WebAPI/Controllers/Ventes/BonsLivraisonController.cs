using GestCom.Application.Features.Ventes.BonsLivraison.Commands.ConvertBLToFacture;
using GestCom.Application.Features.Ventes.BonsLivraison.Commands.CreateBonLivraison;
using GestCom.Application.Features.Ventes.BonsLivraison.Commands.DeleteBonLivraison;
using GestCom.Application.Features.Ventes.BonsLivraison.DTOs;
using GestCom.Application.Features.Ventes.Factures.DTOs;
using GestCom.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestCom.WebAPI.Controllers.Ventes;

/// <summary>
/// Contrôleur pour la gestion des bons de livraison
/// </summary>
[Authorize]
[Route("api/v1/bons-livraison")]
public class BonsLivraisonController : BaseApiController
{
    private readonly IPdfService _pdfService;

    public BonsLivraisonController(IPdfService pdfService)
    {
        _pdfService = pdfService;
    }

    /// <summary>
    /// Récupère la liste des bons de livraison
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BonLivraisonListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BonLivraisonListDto>>> GetAll(
        [FromQuery] string? codeClient,
        [FromQuery] string? statut,
        [FromQuery] DateTime? dateDebut,
        [FromQuery] DateTime? dateFin)
    {
        // À implémenter avec une Query dédiée
        return Ok(Array.Empty<BonLivraisonListDto>());
    }

    /// <summary>
    /// Récupère un bon de livraison par son numéro
    /// </summary>
    [HttpGet("{numero}")]
    [ProducesResponseType(typeof(BonLivraisonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BonLivraisonDto>> GetByNumero(string numero)
    {
        // À implémenter avec une Query dédiée
        return NotFound($"Bon de livraison '{numero}' non trouvé.");
    }

    /// <summary>
    /// Crée un nouveau bon de livraison
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BonLivraisonDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BonLivraisonDto>> Create([FromBody] CreateBonLivraisonCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetByNumero), new { numero = result.NumeroBonLivraison }, result);
    }

    /// <summary>
    /// Convertit un ou plusieurs bons de livraison en facture
    /// </summary>
    [HttpPost("facturer")]
    [ProducesResponseType(typeof(FactureClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FactureClientDto>> ConvertToFacture([FromBody] ConvertBLToFactureCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtRoute("GetFactureByNumero", new { numero = result.NumeroFacture }, result);
    }

    /// <summary>
    /// Supprime un bon de livraison
    /// </summary>
    [HttpDelete("{numero}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(string numero, [FromQuery] bool restaurerStock = true)
    {
        var command = new DeleteBonLivraisonCommand 
        { 
            NumeroBonLivraison = numero,
            RestaurerStock = restaurerStock
        };
        await Mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Génère le PDF d'un bon de livraison
    /// </summary>
    [HttpGet("{numero}/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetPdf(string numero)
    {
        var pdfBytes = await _pdfService.GenerateBonLivraisonPdfAsync(numero);
        return File(pdfBytes, "application/pdf", $"BL_{numero}.pdf");
    }
}
