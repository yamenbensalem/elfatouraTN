using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Ventes.Clients.Queries.GetClientBalance;

public class GetClientBalanceQueryHandler : IRequestHandler<GetClientBalanceQuery, ClientBalanceDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetClientBalanceQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ClientBalanceDto> Handle(GetClientBalanceQuery request, CancellationToken cancellationToken)
    {
        var client = await _unitOfWork.Clients.GetByCodeAsync(request.CodeClient, request.CodeEntreprise);
        if (client == null)
        {
            throw new InvalidOperationException($"Client avec le code '{request.CodeClient}' non trouvé.");
        }

        var factures = await _unitOfWork.FacturesClient.GetFacturesByClientAsync(request.CodeClient, request.CodeEntreprise);
        var facturesClient = factures.ToList();

        var reglements = await _unitOfWork.ReglementsFacture.GetReglementsByClientAsync(request.CodeClient, request.CodeEntreprise);
        var reglementsClient = reglements.ToList();

        var totalFactures = facturesClient.Sum(f => f.MontantTTC);
        var totalReglements = reglementsClient.Sum(r => r.Montant);
        var solde = totalFactures - totalReglements;

        var facturesImpayees = facturesClient
            .Where(f => f.Statut != "Payée")
            .OrderBy(f => f.DateEcheance)
            .Select(f => new FactureClientResumeDto
            {
                NumeroFacture = f.NumeroFacture,
                DateFacture = f.DateFacture,
                DateEcheance = f.DateEcheance,
                MontantTTC = f.MontantTTC,
                MontantRegle = f.APayer - f.MontantRestant, // Computed from APayer - MontantRestant
                ResteAPayer = f.MontantRestant,
                JoursRetard = f.DateEcheance.HasValue && f.DateEcheance < DateTime.Today 
                    ? (DateTime.Today - f.DateEcheance.Value).Days : 0,
                Statut = f.Statut ?? "En attente"
            })
            .ToList();

        return new ClientBalanceDto
        {
            CodeClient = client.CodeClient,
            NomClient = client.Nom,
            LimiteCredit = client.MaxCredit,
            TotalFactures = totalFactures,
            TotalReglements = totalReglements,
            Solde = solde,
            CreditDisponible = client.MaxCredit - solde,
            NombreFacturesImpayees = facturesImpayees.Count,
            FacturesImpayees = facturesImpayees
        };
    }
}
