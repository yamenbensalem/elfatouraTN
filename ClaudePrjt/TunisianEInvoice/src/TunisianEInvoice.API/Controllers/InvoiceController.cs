using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TunisianEInvoice.Application.DTOs;
using TunisianEInvoice.Application.Interfaces;
using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceController(IInvoiceService invoiceService, IInvoiceRepository invoiceRepository)
        {
            _invoiceService = invoiceService;
            _invoiceRepository = invoiceRepository;
        }

        /// <summary>
        /// Get all invoices with pagination and filtering
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<InvoiceRecord>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<InvoiceRecord>>> GetInvoices(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? clientId = null,
            [FromQuery] InvoiceStatus? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var (items, totalCount) = await _invoiceRepository.GetPagedAsync(pageNumber, pageSize, clientId, status, fromDate, toDate);
                var result = new PagedResult<InvoiceRecord>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Erreur: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get invoice by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(InvoiceRecord), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InvoiceRecord>> GetInvoice(Guid id)
        {
            try
            {
                var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(id);
                if (invoice == null)
                {
                    return NotFound(new { error = "Facture non trouvée" });
                }
                return Ok(invoice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Erreur: {ex.Message}" });
            }
        }

        /// <summary>
        /// Generate Tunisian electronic invoice (XML without signature + XML with signature + PDF)
        /// </summary>
        /// <param name="request">Invoice data</param>
        /// <returns>Generated XML files and PDF</returns>
        [HttpPost("generate")]
        [ProducesResponseType(typeof(InvoiceResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<InvoiceResponseDto>> GenerateInvoice(
            [FromBody] InvoiceRequestDto request)
        {
            try
            {
                var result = await _invoiceService.GenerateInvoiceAsync(request);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new InvoiceResponseDto
                {
                    Success = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Generate XML without signature only
        /// </summary>
        /// <param name="request">Invoice data</param>
        /// <returns>XML without signature</returns>
        [HttpPost("generate-xml-unsigned")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> GenerateXmlWithoutSignature(
            [FromBody] InvoiceRequestDto request)
        {
            try
            {
                var xml = await _invoiceService.GenerateXmlWithoutSignatureAsync(request);
                return Ok(xml);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating XML: {ex.Message}");
            }
        }

        /// <summary>
        /// Validate invoice data against XSD schema
        /// </summary>
        /// <param name="request">Invoice data to validate</param>
        /// <returns>Validation result</returns>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(ValidationResultDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ValidationResultDto>> ValidateInvoice(
            [FromBody] InvoiceRequestDto request)
        {
            var validationResult = await _invoiceService.ValidateInvoiceAsync(request);
            return Ok(validationResult);
        }

        /// <summary>
        /// Download PDF invoice
        /// </summary>
        /// <param name="request">Invoice data</param>
        /// <returns>PDF file</returns>
        [HttpPost("generate-pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GeneratePdf([FromBody] InvoiceRequestDto request)
        {
            try
            {
                var pdfBytes = await _invoiceService.GeneratePdfAsync(request);
                return File(pdfBytes, "application/pdf", $"Facture_{request.DocumentIdentifier}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating PDF: {ex.Message}");
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}
