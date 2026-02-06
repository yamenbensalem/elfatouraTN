using GestCom.Application.Features.Ventes.Clients.DTOs;
using MediatR;

namespace GestCom.Application.Features.Ventes.Clients.Queries.GetClientBalance;

/// <summary>
/// Query pour récupérer le solde et l'historique d'un client
/// </summary>
public class GetClientBalanceQuery : IRequest<ClientBalanceDto>
{
    public string CodeClient { get; set; } = string.Empty;
    public string CodeEntreprise { get; set; } = string.Empty;
}

public class ClientBalanceDto
{
    public string CodeClient { get; set; } = string.Empty;
    public string? NomClient { get; set; }
    public decimal LimiteCredit { get; set; }
    public decimal TotalFactures { get; set; }
    public decimal TotalReglements { get; set; }
    public decimal Solde { get; set; }
    public decimal CreditDisponible { get; set; }
    public int NombreFacturesImpayees { get; set; }
    public List<FactureClientResumeDto> FacturesImpayees { get; set; } = new();
}

public class FactureClientResumeDto
{
    public string NumeroFacture { get; set; } = string.Empty;
    public DateTime DateFacture { get; set; }
    public DateTime? DateEcheance { get; set; }
    public decimal MontantTTC { get; set; }
    public decimal MontantRegle { get; set; }
    public decimal ResteAPayer { get; set; }
    public int JoursRetard { get; set; }
    public string Statut { get; set; } = string.Empty;
}
