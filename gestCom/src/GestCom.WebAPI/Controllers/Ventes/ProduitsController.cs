using GestCom.Application.Features.Ventes.Produits.Commands.CreateProduit;
using GestCom.Application.Features.Ventes.Produits.Commands.DeleteProduit;
using GestCom.Application.Features.Ventes.Produits.Commands.UpdateProduit;
using GestCom.Application.Features.Ventes.Produits.DTOs;
using GestCom.Application.Features.Ventes.Produits.Queries.GetAllProduits;
using GestCom.Application.Features.Ventes.Produits.Queries.GetProduitByCode;
using GestCom.Application.Features.Ventes.Produits.Queries.GetProduitsStockFaible;
using GestCom.Application.Features.Ventes.Produits.Queries.SearchProduits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestCom.WebAPI.Controllers.Ventes;

/// <summary>
/// Contrôleur pour la gestion des produits
/// </summary>
[Authorize]
public class ProduitsController : BaseApiController
{
    /// <summary>
    /// Récupère la liste de tous les produits avec filtres optionnels
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProduitListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProduitListDto>>> GetAll([FromQuery] GetAllProduitsQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Récupère un produit par son code
    /// </summary>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(ProduitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProduitDto>> GetByCode(string code)
    {
        var query = new GetProduitByCodeQuery { CodeProduit = code };
        var result = await Mediator.Send(query);
        
        if (result == null)
            return NotFound($"Produit avec le code '{code}' non trouvé.");
            
        return Ok(result);
    }

    /// <summary>
    /// Recherche de produits par terme de recherche
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ProduitListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProduitListDto>>> Search([FromQuery] SearchProduitsQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Récupère les produits avec stock faible ou en rupture
    /// </summary>
    [HttpGet("stock-faible")]
    [ProducesResponseType(typeof(IEnumerable<ProduitStockAlertDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProduitStockAlertDto>>> GetStockFaible([FromQuery] GetProduitsStockFaibleQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Crée un nouveau produit
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProduitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProduitDto>> Create([FromBody] CreateProduitCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetByCode), new { code = result.CodeProduit }, result);
    }

    /// <summary>
    /// Met à jour un produit existant
    /// </summary>
    [HttpPut("{code}")]
    [ProducesResponseType(typeof(ProduitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProduitDto>> Update(string code, [FromBody] UpdateProduitCommand command)
    {
        if (code != command.CodeProduit)
            return BadRequest("Le code du produit dans l'URL ne correspond pas au code dans le body.");

        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Supprime un produit
    /// </summary>
    [HttpDelete("{code}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(string code)
    {
        var command = new DeleteProduitCommand { CodeProduit = code };
        await Mediator.Send(command);
        return NoContent();
    }
}
