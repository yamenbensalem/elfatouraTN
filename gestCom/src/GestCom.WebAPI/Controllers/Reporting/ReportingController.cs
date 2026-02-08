using GestCom.Application.Features.Reporting.Dashboard.Queries.GetDashboardData;
using GestCom.Application.Features.Reporting.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestCom.WebAPI.Controllers.Reporting;

/// <summary>
/// Contrôleur pour le tableau de bord et les rapports
/// </summary>
[Authorize]
[Route("api/v1/reporting")]
public class ReportingController : BaseApiController
{
    /// <summary>
    /// Récupère les données du tableau de bord principal
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardDto>> GetDashboard([FromQuery] DashboardQueryParams queryParams)
    {
        var query = new GetDashboardDataQuery
        {
            DateDebut = queryParams.DateDebut ?? DateTime.Today.AddMonths(-1),
            DateFin = queryParams.DateFin ?? DateTime.Today
        };
        
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Récupère le chiffre d'affaires par période
    /// </summary>
    [HttpGet("chiffre-affaires")]
    [ProducesResponseType(typeof(ChiffreAffairesReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ChiffreAffairesReportDto>> GetChiffreAffaires([FromQuery] PeriodeQueryParams queryParams)
    {
        // À implémenter
        var result = new ChiffreAffairesReportDto
        {
            DateDebut = queryParams.DateDebut ?? DateTime.Today.AddMonths(-12),
            DateFin = queryParams.DateFin ?? DateTime.Today,
            TotalHT = 0,
            TotalTTC = 0,
            ChiffreAffairesParMois = new List<ChiffreAffairesMensuelDto>()
        };
        return Ok(result);
    }

    /// <summary>
    /// Récupère le rapport des créances clients
    /// </summary>
    [HttpGet("creances")]
    [ProducesResponseType(typeof(CreancesReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CreancesReportDto>> GetCreances([FromQuery] CreancesQueryParams? queryParams)
    {
        // À implémenter
        var result = new CreancesReportDto
        {
            TotalCreances = 0,
            CreancesNonEchues = 0,
            CreancesEchues = 0,
            Creances0_30Jours = 0,
            Creances30_60Jours = 0,
            Creances60_90Jours = 0,
            CreancesPlus90Jours = 0,
            DetailParClient = new List<CreanceClientDto>()
        };
        return Ok(result);
    }

    /// <summary>
    /// Récupère le rapport des dettes fournisseurs
    /// </summary>
    [HttpGet("dettes")]
    [ProducesResponseType(typeof(DettesReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DettesReportDto>> GetDettes([FromQuery] DettesQueryParams? queryParams)
    {
        // À implémenter
        var result = new DettesReportDto
        {
            TotalDettes = 0,
            DettesNonEchues = 0,
            DettesEchues = 0,
            DetailParFournisseur = new List<DetteFournisseurDto>()
        };
        return Ok(result);
    }

    /// <summary>
    /// Récupère le rapport d'état du stock
    /// </summary>
    [HttpGet("stock")]
    [ProducesResponseType(typeof(StockReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<StockReportDto>> GetStock([FromQuery] StockQueryParams? queryParams)
    {
        // À implémenter
        var result = new StockReportDto
        {
            ValeurTotaleStock = 0,
            NombreProduits = 0,
            ProduitsEnStock = 0,
            ProduitsEnRupture = 0,
            ProduitsStockFaible = 0,
            DetailParCategorie = new List<StockCategorieDto>()
        };
        return Ok(result);
    }

    /// <summary>
    /// Récupère le top des produits les plus vendus
    /// </summary>
    [HttpGet("top-produits")]
    [ProducesResponseType(typeof(IEnumerable<TopProduitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TopProduitDto>>> GetTopProduits([FromQuery] TopProduitsQueryParams queryParams)
    {
        // À implémenter
        var result = new List<TopProduitDto>();
        return Ok(result);
    }

    /// <summary>
    /// Récupère le top des clients
    /// </summary>
    [HttpGet("top-clients")]
    [ProducesResponseType(typeof(IEnumerable<TopClientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TopClientDto>>> GetTopClients([FromQuery] TopClientsQueryParams queryParams)
    {
        // À implémenter
        var result = new List<TopClientDto>();
        return Ok(result);
    }

    /// <summary>
    /// Récupère les statistiques de vente par catégorie
    /// </summary>
    [HttpGet("ventes-par-categorie")]
    [ProducesResponseType(typeof(IEnumerable<VenteParCategorieDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VenteParCategorieDto>>> GetVentesParCategorie([FromQuery] PeriodeQueryParams queryParams)
    {
        // À implémenter
        var result = new List<VenteParCategorieDto>();
        return Ok(result);
    }

    /// <summary>
    /// Récupère la marge brute par période
    /// </summary>
    [HttpGet("marge-brute")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(MargeBruteReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MargeBruteReportDto>> GetMargeBrute([FromQuery] PeriodeQueryParams queryParams)
    {
        // À implémenter
        var result = new MargeBruteReportDto
        {
            DateDebut = queryParams.DateDebut ?? DateTime.Today.AddMonths(-12),
            DateFin = queryParams.DateFin ?? DateTime.Today,
            ChiffreAffairesHT = 0,
            CoutAchat = 0,
            MargeBrute = 0,
            TauxMarge = 0
        };
        return Ok(result);
    }

    /// <summary>
    /// Exporte un rapport en PDF
    /// </summary>
    [HttpGet("export/{type}")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ExportReport(string type, [FromQuery] PeriodeQueryParams queryParams)
    {
        var typesValides = new[] { "chiffre-affaires", "creances", "dettes", "stock", "marge-brute" };
        if (!typesValides.Contains(type.ToLower()))
            return BadRequest($"Type de rapport invalide. Types valides: {string.Join(", ", typesValides)}");

        // À implémenter via IPdfService
        var pdfContent = Array.Empty<byte>();
        var fileName = $"Rapport_{type}_{DateTime.Now:yyyyMMdd}.pdf";
        
        return File(pdfContent, "application/pdf", fileName);
    }
}

#region Query Params

public class DashboardQueryParams
{
    public DateTime? DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
}

public class PeriodeQueryParams
{
    public DateTime? DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
}

public class CreancesQueryParams
{
    public string? CodeClient { get; set; }
    public bool? EchuesUniquement { get; set; }
}

public class DettesQueryParams
{
    public string? CodeFournisseur { get; set; }
    public bool? EchuesUniquement { get; set; }
}

public class StockQueryParams
{
    public string? CodeCategorie { get; set; }
    public string? CodeMagasin { get; set; }
    public bool? RuptureUniquement { get; set; }
    public bool? StockFaibleUniquement { get; set; }
}

public class TopProduitsQueryParams
{
    public DateTime? DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
    public int Top { get; set; } = 10;
    public string? CodeCategorie { get; set; }
}

public class TopClientsQueryParams
{
    public DateTime? DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
    public int Top { get; set; } = 10;
}

#endregion

