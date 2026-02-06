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

#region Report DTOs

public class ChiffreAffairesReportDto
{
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public decimal TotalHT { get; set; }
    public decimal TotalTTC { get; set; }
    public List<ChiffreAffairesMensuelDto> ChiffreAffairesParMois { get; set; } = new();
}

public class ChiffreAffairesMensuelDto
{
    public int Annee { get; set; }
    public int Mois { get; set; }
    public string NomMois { get; set; } = string.Empty;
    public decimal MontantHT { get; set; }
    public decimal MontantTTC { get; set; }
    public int NombreFactures { get; set; }
}

public class CreancesReportDto
{
    public decimal TotalCreances { get; set; }
    public decimal CreancesNonEchues { get; set; }
    public decimal CreancesEchues { get; set; }
    public decimal Creances0_30Jours { get; set; }
    public decimal Creances30_60Jours { get; set; }
    public decimal Creances60_90Jours { get; set; }
    public decimal CreancesPlus90Jours { get; set; }
    public List<CreanceClientDto> DetailParClient { get; set; } = new();
}

public class CreanceClientDto
{
    public string CodeClient { get; set; } = string.Empty;
    public string NomClient { get; set; } = string.Empty;
    public decimal MontantTotal { get; set; }
    public decimal MontantEchu { get; set; }
    public int NombreFacturesImpayees { get; set; }
    public DateTime? DateDerniereFacture { get; set; }
}

public class DettesReportDto
{
    public decimal TotalDettes { get; set; }
    public decimal DettesNonEchues { get; set; }
    public decimal DettesEchues { get; set; }
    public List<DetteFournisseurDto> DetailParFournisseur { get; set; } = new();
}

public class DetteFournisseurDto
{
    public string CodeFournisseur { get; set; } = string.Empty;
    public string NomFournisseur { get; set; } = string.Empty;
    public decimal MontantTotal { get; set; }
    public decimal MontantEchu { get; set; }
    public int NombreFacturesImpayees { get; set; }
}

public class StockReportDto
{
    public decimal ValeurTotaleStock { get; set; }
    public int NombreProduits { get; set; }
    public int ProduitsEnStock { get; set; }
    public int ProduitsEnRupture { get; set; }
    public int ProduitsStockFaible { get; set; }
    public List<StockCategorieDto> DetailParCategorie { get; set; } = new();
}

public class StockCategorieDto
{
    public string CodeCategorie { get; set; } = string.Empty;
    public string NomCategorie { get; set; } = string.Empty;
    public int NombreProduits { get; set; }
    public decimal ValeurStock { get; set; }
    public int ProduitsEnRupture { get; set; }
}

public class TopProduitDto
{
    public string CodeProduit { get; set; } = string.Empty;
    public string LibelleProduit { get; set; } = string.Empty;
    public string? CodeCategorie { get; set; }
    public decimal QuantiteVendue { get; set; }
    public decimal ChiffreAffaires { get; set; }
    public decimal Marge { get; set; }
}

public class TopClientDto
{
    public string CodeClient { get; set; } = string.Empty;
    public string NomClient { get; set; } = string.Empty;
    public decimal ChiffreAffaires { get; set; }
    public int NombreFactures { get; set; }
    public decimal MontantMoyenFacture { get; set; }
}

public class VenteParCategorieDto
{
    public string CodeCategorie { get; set; } = string.Empty;
    public string NomCategorie { get; set; } = string.Empty;
    public decimal ChiffreAffaires { get; set; }
    public decimal Pourcentage { get; set; }
    public int NombreProduits { get; set; }
}

public class MargeBruteReportDto
{
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public decimal ChiffreAffairesHT { get; set; }
    public decimal CoutAchat { get; set; }
    public decimal MargeBrute { get; set; }
    public decimal TauxMarge { get; set; }
}

#endregion
