using GestCom.Application.Features.Reporting.DTOs;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Reporting.Queries.GetRapportCreances;

public class GetRapportCreancesQueryHandler : IRequestHandler<GetRapportCreancesQuery, RapportCreancesDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRapportCreancesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RapportCreancesDto> Handle(GetRapportCreancesQuery request, CancellationToken cancellationToken)
    {
        var dateRef = request.DateReference ?? DateTime.Today;
        var rapport = new RapportCreancesDto();

        // Récupérer les factures impayées
        var factures = await _unitOfWork.FacturesClient.GetAllAsync();
        var facturesImpayees = factures
            .Where(f => f.Statut != "Payée")
            .ToList();

        // Filtrer par client si spécifié
        if (!string.IsNullOrEmpty(request.CodeClient))
        {
            facturesImpayees = facturesImpayees.Where(f => f.CodeClient == request.CodeClient).ToList();
        }

        // Filtrer uniquement échues si demandé
        if (request.SeulementEchues)
        {
            facturesImpayees = facturesImpayees.Where(f => f.DateEcheance < dateRef).ToList();
        }

        // Calculer les créances
        foreach (var facture in facturesImpayees)
        {
            var resteAPayer = facture.MontantTTC - facture.MontantRegle;
            rapport.TotalCreances += resteAPayer;

            if (facture.DateEcheance >= dateRef)
            {
                rapport.CreancesNonEchues += resteAPayer;
            }
            else
            {
                rapport.CreancesEchues += resteAPayer;
                var joursRetard = facture.DateEcheance.HasValue ? (dateRef - facture.DateEcheance.Value).Days : 0;

                if (joursRetard <= 30)
                {
                    rapport.CreancesEchues30Jours += resteAPayer;
                }
                else if (joursRetard <= 60)
                {
                    rapport.CreancesEchues60Jours += resteAPayer;
                }
                else if (joursRetard <= 90)
                {
                    rapport.CreancesEchues90Jours += resteAPayer;
                }
                else
                {
                    rapport.CreancesEchuesPlus90Jours += resteAPayer;
                }
            }
        }

        // Récupérer les clients pour les noms
        var clients = await _unitOfWork.Clients.GetAllAsync();
        var clientsDict = clients.ToDictionary(c => c.CodeClient, c => c.NomClient);

        // Créances par client
        rapport.ParClient = facturesImpayees
            .GroupBy(f => f.CodeClient)
            .Select(g =>
            {
                var facturesClient = g.ToList();
                var plusAncienne = facturesClient
                    .Where(f => f.DateEcheance < dateRef)
                    .OrderBy(f => f.DateEcheance)
                    .FirstOrDefault();

                return new CreanceClientDto
                {
                    CodeClient = g.Key,
                    NomClient = clientsDict.GetValueOrDefault(g.Key),
                    TotalCreances = g.Sum(f => f.MontantTTC - f.MontantRegle),
                    NombreFacturesImpayees = g.Count(),
                    DatePlusAncienneFacture = plusAncienne?.DateEcheance,
                    JoursRetard = plusAncienne?.DateEcheance.HasValue == true ? (dateRef - plusAncienne.DateEcheance.Value).Days : 0
                };
            })
            .OrderByDescending(c => c.TotalCreances)
            .ToList();

        return rapport;
    }
}
