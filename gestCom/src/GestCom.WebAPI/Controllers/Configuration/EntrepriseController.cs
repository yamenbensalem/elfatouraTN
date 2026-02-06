using GestCom.Application.Features.Configuration.DTOs;
using GestCom.Application.Features.Configuration.Entreprise.Commands.UpdateEntreprise;
using GestCom.Application.Features.Configuration.Entreprise.Queries.GetEntreprise;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestCom.WebAPI.Controllers.Configuration;

/// <summary>
/// Contrôleur pour la gestion des paramètres de l'entreprise
/// </summary>
[Authorize]
[Route("api/v1/entreprise")]
public class EntrepriseController : BaseApiController
{
    /// <summary>
    /// Récupère les informations de l'entreprise courante
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(EntrepriseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EntrepriseDto>> Get()
    {
        var query = new GetEntrepriseQuery();
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Met à jour les informations de l'entreprise
    /// </summary>
    [HttpPut]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(EntrepriseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EntrepriseDto>> Update([FromBody] UpdateEntrepriseCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Met à jour le logo de l'entreprise
    /// </summary>
    [HttpPost("logo")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateLogo(IFormFile logo)
    {
        if (logo == null || logo.Length == 0)
            return BadRequest("Aucun fichier logo fourni.");

        // Validation du type de fichier
        var allowedTypes = new[] { "image/png", "image/jpeg", "image/gif" };
        if (!allowedTypes.Contains(logo.ContentType.ToLower()))
            return BadRequest("Type de fichier non autorisé. Formats acceptés: PNG, JPEG, GIF.");

        // Validation de la taille (max 2MB)
        if (logo.Length > 2 * 1024 * 1024)
            return BadRequest("Le fichier est trop volumineux. Taille maximale: 2MB.");

        // À implémenter - sauvegarder le logo
        using var memoryStream = new MemoryStream();
        await logo.CopyToAsync(memoryStream);
        var logoBase64 = Convert.ToBase64String(memoryStream.ToArray());

        return Ok(new { message = "Logo mis à jour avec succès", logoBase64 });
    }

    /// <summary>
    /// Récupère les paramètres de décimales
    /// </summary>
    [HttpGet("parametres-decimales")]
    [ProducesResponseType(typeof(ParametresDecimalesDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ParametresDecimalesDto>> GetParametresDecimales()
    {
        // À implémenter via query
        var result = new ParametresDecimalesDto
        {
            DecimalesQuantite = 3,
            DecimalesPrix = 3,
            DecimalesMontant = 3,
            DecimalesTva = 2
        };
        return Ok(result);
    }

    /// <summary>
    /// Met à jour les paramètres de décimales
    /// </summary>
    [HttpPut("parametres-decimales")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ParametresDecimalesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ParametresDecimalesDto>> UpdateParametresDecimales([FromBody] ParametresDecimalesDto dto)
    {
        // À implémenter
        return Ok(dto);
    }

    /// <summary>
    /// Récupère les séries de numérotation
    /// </summary>
    [HttpGet("numerotation")]
    [ProducesResponseType(typeof(IEnumerable<SerieNumerotationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SerieNumerotationDto>>> GetNumerotation()
    {
        // À implémenter
        var result = new List<SerieNumerotationDto>
        {
            new() { TypeDocument = "FactureClient", Prefixe = "FC", DernierNumero = 1, Format = "{Prefixe}{Annee}-{Numero:00000}" },
            new() { TypeDocument = "DevisClient", Prefixe = "DV", DernierNumero = 1, Format = "{Prefixe}{Annee}-{Numero:00000}" },
            new() { TypeDocument = "BonLivraison", Prefixe = "BL", DernierNumero = 1, Format = "{Prefixe}{Annee}-{Numero:00000}" },
            new() { TypeDocument = "CommandeVente", Prefixe = "CV", DernierNumero = 1, Format = "{Prefixe}{Annee}-{Numero:00000}" },
            new() { TypeDocument = "FactureFournisseur", Prefixe = "FF", DernierNumero = 1, Format = "{Prefixe}{Annee}-{Numero:00000}" },
            new() { TypeDocument = "CommandeAchat", Prefixe = "CA", DernierNumero = 1, Format = "{Prefixe}{Annee}-{Numero:00000}" },
            new() { TypeDocument = "BonReception", Prefixe = "BR", DernierNumero = 1, Format = "{Prefixe}{Annee}-{Numero:00000}" }
        };
        return Ok(result);
    }

    /// <summary>
    /// Met à jour une série de numérotation
    /// </summary>
    [HttpPut("numerotation/{typeDocument}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SerieNumerotationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SerieNumerotationDto>> UpdateNumerotation(
        string typeDocument, 
        [FromBody] UpdateSerieNumerotationDto dto)
    {
        // À implémenter
        var result = new SerieNumerotationDto
        {
            TypeDocument = typeDocument,
            Prefixe = dto.Prefixe,
            DernierNumero = dto.DernierNumero,
            Format = dto.Format
        };
        return Ok(result);
    }
}

public class ParametresDecimalesDto
{
    public int DecimalesQuantite { get; set; } = 3;
    public int DecimalesPrix { get; set; } = 3;
    public int DecimalesMontant { get; set; } = 3;
    public int DecimalesTva { get; set; } = 2;
}

public class SerieNumerotationDto
{
    public string TypeDocument { get; set; } = string.Empty;
    public string Prefixe { get; set; } = string.Empty;
    public int DernierNumero { get; set; }
    public string Format { get; set; } = string.Empty;
}

public class UpdateSerieNumerotationDto
{
    public string Prefixe { get; set; } = string.Empty;
    public int DernierNumero { get; set; }
    public string Format { get; set; } = string.Empty;
}
