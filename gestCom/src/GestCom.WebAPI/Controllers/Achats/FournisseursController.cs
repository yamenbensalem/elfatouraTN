using GestCom.Application.Features.Achats.Fournisseurs.Commands.CreateFournisseur;
using GestCom.Application.Features.Achats.Fournisseurs.DTOs;
using GestCom.Application.Features.Achats.Fournisseurs.Queries.GetAllFournisseurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestCom.WebAPI.Controllers.Achats;

/// <summary>
/// Contrôleur pour la gestion des fournisseurs
/// </summary>
[Authorize]
public class FournisseursController : BaseApiController
{
    /// <summary>
    /// Récupère la liste de tous les fournisseurs
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FournisseurListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FournisseurListDto>>> GetAll([FromQuery] GetAllFournisseursQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Récupère un fournisseur par son code
    /// </summary>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(FournisseurDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FournisseurDto>> GetByCode(string code)
    {
        // À implémenter avec une Query dédiée
        return NotFound($"Fournisseur avec le code '{code}' non trouvé.");
    }

    /// <summary>
    /// Récupère le solde d'un fournisseur
    /// </summary>
    [HttpGet("{code}/balance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetBalance(string code)
    {
        // À implémenter avec une Query dédiée
        return Ok(new 
        { 
            CodeFournisseur = code, 
            TotalAchats = 0m, 
            TotalReglements = 0m, 
            Solde = 0m 
        });
    }

    /// <summary>
    /// Crée un nouveau fournisseur
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(FournisseurDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FournisseurDto>> Create([FromBody] CreateFournisseurCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetByCode), new { code = result.CodeFournisseur }, result);
    }

    /// <summary>
    /// Met à jour un fournisseur
    /// </summary>
    [HttpPut("{code}")]
    [ProducesResponseType(typeof(FournisseurDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FournisseurDto>> Update(string code, [FromBody] UpdateFournisseurDto dto)
    {
        // À implémenter
        return NotFound($"Fournisseur '{code}' non trouvé.");
    }

    /// <summary>
    /// Supprime un fournisseur
    /// </summary>
    [HttpDelete("{code}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(string code)
    {
        // À implémenter
        return NoContent();
    }
}
