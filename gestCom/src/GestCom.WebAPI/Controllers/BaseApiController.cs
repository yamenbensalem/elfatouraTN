using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GestCom.WebAPI.Controllers;

/// <summary>
/// Contrôleur de base pour tous les contrôleurs API
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;

    /// <summary>
    /// Instance de MediatR (lazy-loaded)
    /// </summary>
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>
    /// Retourne une réponse 201 Created avec l'URL de la ressource
    /// </summary>
    protected CreatedAtActionResult CreatedAtAction<T>(string actionName, object routeValues, T value)
    {
        return base.CreatedAtAction(actionName, routeValues, value);
    }

    /// <summary>
    /// Retourne une réponse 404 NotFound avec un message personnalisé
    /// </summary>
    protected NotFoundObjectResult NotFound(string message)
    {
        return base.NotFound(new { message });
    }

    /// <summary>
    /// Retourne une réponse 400 BadRequest avec un message personnalisé
    /// </summary>
    protected BadRequestObjectResult BadRequest(string message)
    {
        return base.BadRequest(new { message });
    }
}
