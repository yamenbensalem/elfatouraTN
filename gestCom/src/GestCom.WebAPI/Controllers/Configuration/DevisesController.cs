using GestCom.Application.Features.Configuration.Devises.Queries.GetAllDevises;
using GestCom.Application.Features.Configuration.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestCom.WebAPI.Controllers.Configuration;

/// <summary>
/// Contrôleur pour la gestion des devises
/// </summary>
[Authorize]
public class DevisesController : BaseApiController
{
    /// <summary>
    /// Récupère toutes les devises
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DeviseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DeviseDto>>> GetAll()
    {
        var query = new GetAllDevisesQuery();
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Récupère une devise par son code
    /// </summary>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(DeviseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeviseDto>> GetByCode(string code)
    {
        var query = new GetAllDevisesQuery();
        var result = await Mediator.Send(query);
        var devise = result.FirstOrDefault(d => d.CodeDevise == code);
        
        if (devise == null)
            return NotFound($"Devise '{code}' non trouvée.");
            
        return Ok(devise);
    }

    /// <summary>
    /// Récupère la devise par défaut (TND)
    /// </summary>
    [HttpGet("defaut")]
    [ProducesResponseType(typeof(DeviseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DeviseDto>> GetDeviseDefaut()
    {
        var query = new GetAllDevisesQuery();
        var result = await Mediator.Send(query);
        var deviseDefaut = result.FirstOrDefault(d => d.CodeDevise == "TND");
        
        if (deviseDefaut == null)
        {
            // Retourner TND par défaut
            return Ok(new DeviseDto
            {
                CodeDevise = "TND",
                LibelleDevise = "Dinar Tunisien",
                TauxChange = 1.0m,
                Symbole = "DT"
            });
        }
        
        return Ok(deviseDefaut);
    }
}
