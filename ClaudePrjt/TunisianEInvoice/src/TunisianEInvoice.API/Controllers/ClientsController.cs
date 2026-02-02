using Microsoft.AspNetCore.Mvc;
using TunisianEInvoice.Application.Interfaces;
using TunisianEInvoice.Domain.Entities;

namespace TunisianEInvoice.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<ClientsController> _logger;

    public ClientsController(IClientRepository clientRepository, ILogger<ClientsController> logger)
    {
        _clientRepository = clientRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all clients
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Client>>> GetClients()
    {
        try
        {
            var clients = await _clientRepository.GetAllAsync();
            return Ok(clients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving clients");
            return StatusCode(500, new { error = "Erreur lors de la récupération des clients" });
        }
    }

    /// <summary>
    /// Get client by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Client>> GetClient(Guid id)
    {
        try
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
            {
                return NotFound(new { error = "Client non trouvé" });
            }
            return Ok(client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving client {Id}", id);
            return StatusCode(500, new { error = "Erreur lors de la récupération du client" });
        }
    }

    /// <summary>
    /// Get client by Matricule Fiscal
    /// </summary>
    [HttpGet("by-mf/{matriculeFiscal}")]
    public async Task<ActionResult<Client>> GetClientByMatricule(string matriculeFiscal)
    {
        try
        {
            var client = await _clientRepository.GetByMatriculeFiscalAsync(matriculeFiscal);
            if (client == null)
            {
                return NotFound(new { error = "Client non trouvé" });
            }
            return Ok(client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving client by MF {MF}", matriculeFiscal);
            return StatusCode(500, new { error = "Erreur lors de la récupération du client" });
        }
    }

    /// <summary>
    /// Search clients by name or matricule
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Client>>> SearchClients([FromQuery] string term)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return BadRequest(new { error = "Terme de recherche requis" });
            }
            
            var clients = await _clientRepository.SearchAsync(term);
            return Ok(clients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching clients with term {Term}", term);
            return StatusCode(500, new { error = "Erreur lors de la recherche" });
        }
    }

    /// <summary>
    /// Create a new client
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Client>> CreateClient([FromBody] CreateClientRequest request)
    {
        try
        {
            // Check if matricule fiscal already exists
            var existing = await _clientRepository.GetByMatriculeFiscalAsync(request.MatriculeFiscal);
            if (existing != null)
            {
                return BadRequest(new { error = "Un client avec ce matricule fiscal existe déjà" });
            }

            var client = new Client
            {
                Id = Guid.NewGuid(),
                MatriculeFiscal = request.MatriculeFiscal,
                Name = request.Name,
                LegalForm = request.LegalForm,
                RegistrationNumber = request.RegistrationNumber,
                Capital = request.Capital,
                AddressDescription = request.AddressDescription,
                Street = request.Street,
                City = request.City,
                PostalCode = request.PostalCode,
                CountryCode = request.CountryCode ?? "TN",
                Phone = request.Phone,
                Fax = request.Fax,
                Email = request.Email,
                Website = request.Website,
                BankCode = request.BankCode,
                BankAccountNumber = request.BankAccountNumber,
                BankName = request.BankName,
                TtnAccountMode = request.TtnAccountMode ?? "TEST",
                TtnAccountRank = request.TtnAccountRank ?? "1",
                TtnProfile = request.TtnProfile,
                TtnClientCode = request.TtnClientCode,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _clientRepository.AddAsync(client);
            
            return CreatedAtAction(nameof(GetClient), new { id = client.Id }, client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating client");
            return StatusCode(500, new { error = "Erreur lors de la création du client" });
        }
    }

    /// <summary>
    /// Update an existing client
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Client>> UpdateClient(Guid id, [FromBody] UpdateClientRequest request)
    {
        try
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
            {
                return NotFound(new { error = "Client non trouvé" });
            }

            // Update fields
            client.Name = request.Name ?? client.Name;
            client.LegalForm = request.LegalForm ?? client.LegalForm;
            client.RegistrationNumber = request.RegistrationNumber ?? client.RegistrationNumber;
            client.Capital = request.Capital ?? client.Capital;
            client.AddressDescription = request.AddressDescription ?? client.AddressDescription;
            client.Street = request.Street ?? client.Street;
            client.City = request.City ?? client.City;
            client.PostalCode = request.PostalCode ?? client.PostalCode;
            client.Phone = request.Phone ?? client.Phone;
            client.Fax = request.Fax ?? client.Fax;
            client.Email = request.Email ?? client.Email;
            client.Website = request.Website ?? client.Website;
            client.BankCode = request.BankCode ?? client.BankCode;
            client.BankAccountNumber = request.BankAccountNumber ?? client.BankAccountNumber;
            client.BankName = request.BankName ?? client.BankName;
            client.TtnAccountMode = request.TtnAccountMode ?? client.TtnAccountMode;
            client.TtnAccountRank = request.TtnAccountRank ?? client.TtnAccountRank;
            client.TtnProfile = request.TtnProfile ?? client.TtnProfile;
            client.TtnClientCode = request.TtnClientCode ?? client.TtnClientCode;
            client.IsActive = request.IsActive ?? client.IsActive;

            await _clientRepository.UpdateAsync(client);
            
            return Ok(client);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating client {Id}", id);
            return StatusCode(500, new { error = "Erreur lors de la mise à jour du client" });
        }
    }

    /// <summary>
    /// Delete a client
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteClient(Guid id)
    {
        try
        {
            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
            {
                return NotFound(new { error = "Client non trouvé" });
            }

            await _clientRepository.DeleteAsync(id);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting client {Id}", id);
            return StatusCode(500, new { error = "Erreur lors de la suppression du client" });
        }
    }

    /// <summary>
    /// Check if matricule fiscal exists
    /// </summary>
    [HttpGet("check-mf/{matriculeFiscal}")]
    public async Task<ActionResult<bool>> CheckMatriculeFiscalExists(string matriculeFiscal)
    {
        try
        {
            var exists = await _clientRepository.ExistsAsync(matriculeFiscal);
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking MF {MF}", matriculeFiscal);
            return StatusCode(500, new { error = "Erreur lors de la vérification" });
        }
    }
}

public class CreateClientRequest
{
    public required string MatriculeFiscal { get; set; }
    public required string Name { get; set; }
    public string? LegalForm { get; set; }
    public string? RegistrationNumber { get; set; }
    public decimal? Capital { get; set; }
    public string? AddressDescription { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? CountryCode { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? BankCode { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? TtnAccountMode { get; set; }
    public string? TtnAccountRank { get; set; }
    public string? TtnProfile { get; set; }
    public string? TtnClientCode { get; set; }
}

public class UpdateClientRequest
{
    public string? Name { get; set; }
    public string? LegalForm { get; set; }
    public string? RegistrationNumber { get; set; }
    public decimal? Capital { get; set; }
    public string? AddressDescription { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? BankCode { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? TtnAccountMode { get; set; }
    public string? TtnAccountRank { get; set; }
    public string? TtnProfile { get; set; }
    public string? TtnClientCode { get; set; }
    public bool? IsActive { get; set; }
}
