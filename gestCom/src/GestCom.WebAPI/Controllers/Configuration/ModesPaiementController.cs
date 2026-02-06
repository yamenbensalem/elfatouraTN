using GestCom.Application.Features.Configuration.DTOs;
using GestCom.Application.Features.Configuration.ModesPaiement.Queries.GetAllModesPaiement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestCom.WebAPI.Controllers.Configuration;

/// <summary>
/// Contrôleur pour la gestion des modes de paiement
/// </summary>
[Authorize]
public class ModesPaiementController : BaseApiController
{
    /// <summary>
    /// Récupère tous les modes de paiement
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ModePayementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ModePayementDto>>> GetAll()
    {
        var query = new GetAllModesPaiementQuery();
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Récupère un mode de paiement par son code
    /// </summary>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(ModePayementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ModePayementDto>> GetByCode(string code)
    {
        var query = new GetAllModesPaiementQuery();
        var result = await Mediator.Send(query);
        var mode = result.FirstOrDefault(m => m.CodeModePaiement == code);
        
        if (mode == null)
            return NotFound($"Mode de paiement '{code}' non trouvé.");
            
        return Ok(mode);
    }

    /// <summary>
    /// Crée un nouveau mode de paiement
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ModePayementDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ModePayementDto>> Create([FromBody] CreateModePayementDto dto)
    {
        // À implémenter
        var result = new ModePayementDto
        {
            CodeModePaiement = dto.CodeModePaiement,
            LibelleModePaiement = dto.LibelleModePaiement,
            NecessiteReference = dto.NecessiteReference
        };
        return CreatedAtAction(nameof(GetByCode), new { code = result.CodeModePaiement }, result);
    }

    /// <summary>
    /// Met à jour un mode de paiement
    /// </summary>
    [HttpPut("{code}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ModePayementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ModePayementDto>> Update(string code, [FromBody] UpdateModePayementDto dto)
    {
        // À implémenter
        return NotFound($"Mode de paiement '{code}' non trouvé.");
    }

    /// <summary>
    /// Supprime un mode de paiement
    /// </summary>
    [HttpDelete("{code}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete(string code)
    {
        // À implémenter - vérifier qu'il n'y a pas de règlements liés
        return NoContent();
    }
}

public class CreateModePayementDto
{
    public string CodeModePaiement { get; set; } = string.Empty;
    public string LibelleModePaiement { get; set; } = string.Empty;
    public bool NecessiteReference { get; set; }
}

public class UpdateModePayementDto
{
    public string LibelleModePaiement { get; set; } = string.Empty;
    public bool NecessiteReference { get; set; }
    public bool EstActif { get; set; } = true;
}
