using GestCom.Application.Features.Configuration.DTOs;
using GestCom.Application.Features.Configuration.Magasins.Commands.CreateMagasin;
using GestCom.Application.Features.Configuration.Magasins.Queries.GetAllMagasins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestCom.WebAPI.Controllers.Configuration;

/// <summary>
/// Contrôleur pour la gestion des magasins/dépôts
/// </summary>
[Authorize]
public class MagasinsController : BaseApiController
{
    /// <summary>
    /// Récupère tous les magasins
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MagasinProduitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MagasinProduitDto>>> GetAll()
    {
        var query = new GetAllMagasinsQuery();
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Récupère un magasin par son code
    /// </summary>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(MagasinProduitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MagasinProduitDto>> GetByCode(string code)
    {
        var query = new GetAllMagasinsQuery();
        var result = await Mediator.Send(query);
        var magasin = result.FirstOrDefault(m => m.CodeMagasin == code);
        
        if (magasin == null)
            return NotFound($"Magasin '{code}' non trouvé.");
            
        return Ok(magasin);
    }

    /// <summary>
    /// Récupère le magasin par défaut
    /// </summary>
    [HttpGet("defaut")]
    [ProducesResponseType(typeof(MagasinProduitDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MagasinProduitDto>> GetMagasinDefaut()
    {
        var query = new GetAllMagasinsQuery();
        var result = await Mediator.Send(query);
        var magasinDefaut = result.FirstOrDefault(m => m.EstDefaut) ?? result.FirstOrDefault();
        
        if (magasinDefaut == null)
        {
            return Ok(new MagasinProduitDto
            {
                CodeMagasin = "PRINCIPAL",
                LibelleMagasin = "Magasin Principal",
                EstDefaut = true,
                EstActif = true
            });
        }
        
        return Ok(magasinDefaut);
    }

    /// <summary>
    /// Crée un nouveau magasin
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(MagasinProduitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MagasinProduitDto>> Create([FromBody] CreateMagasinCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetByCode), new { code = result.CodeMagasin }, result);
    }

    /// <summary>
    /// Met à jour un magasin
    /// </summary>
    [HttpPut("{code}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(MagasinProduitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MagasinProduitDto>> Update(string code, [FromBody] UpdateMagasinDto dto)
    {
        // À implémenter
        return NotFound($"Magasin '{code}' non trouvé.");
    }

    /// <summary>
    /// Désactive un magasin
    /// </summary>
    [HttpPatch("{code}/desactiver")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Desactiver(string code)
    {
        // À implémenter
        return Ok();
    }

    /// <summary>
    /// Définit un magasin comme défaut
    /// </summary>
    [HttpPatch("{code}/definir-defaut")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DefinirDefaut(string code)
    {
        // À implémenter
        return Ok();
    }

    /// <summary>
    /// Supprime un magasin
    /// </summary>
    [HttpDelete("{code}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete(string code)
    {
        // À implémenter - vérifier qu'il n'y a pas de stock dans ce magasin
        return NoContent();
    }
}

public class UpdateMagasinDto
{
    public string LibelleMagasin { get; set; } = string.Empty;
    public string? Adresse { get; set; }
    public string? Responsable { get; set; }
    public bool EstActif { get; set; } = true;
}
