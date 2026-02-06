using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Ventes.Factures.Queries.GetFacturesEchues;

public class GetFacturesEchuesQueryHandler : IRequestHandler<GetFacturesEchuesQuery, IEnumerable<FactureEchueDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFacturesEchuesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<FactureEchueDto>> Handle(GetFacturesEchuesQuery request, CancellationToken cancellationToken)
    {
        var dateRef = request.DateReference ?? DateTime.Today;

        var factures = await _unitOfWork.FacturesClient.GetAllAsync();
        var facturesEchues = factures
            .Where(f => f.Statut != "Payée" && f.DateEcheance < dateRef)
            .ToList();

        // Filtrer par client
        if (!string.IsNullOrEmpty(request.CodeClient))
        {
            facturesEchues = facturesEchues.Where(f => f.CodeClient == request.CodeClient).ToList();
        }

        // Filtrer par jours de retard minimum
        if (request.JoursRetardMinimum.HasValue)
        {
            facturesEchues = facturesEchues
                .Where(f => f.DateEcheance.HasValue && (dateRef - f.DateEcheance.Value).Days >= request.JoursRetardMinimum.Value)
                .ToList();
        }

        // Récupérer les clients
        var clients = await _unitOfWork.Clients.GetAllAsync();
        var clientsDict = clients.ToDictionary(c => c.CodeClient, c => c);

        return facturesEchues
            .Select(f =>
            {
                var client = clientsDict.GetValueOrDefault(f.CodeClient);
                var joursRetard = f.DateEcheance.HasValue ? (dateRef - f.DateEcheance.Value).Days : 0;
                
                return new FactureEchueDto
                {
                    NumeroFacture = f.NumeroFacture,
                    DateFacture = f.DateFacture,
                    DateEcheance = f.DateEcheance ?? DateTime.MinValue,
                    CodeClient = f.CodeClient,
                    NomClient = client?.NomClient,
                    Telephone = client?.Telephone,
                    Email = client?.Email,
                    MontantTTC = f.MontantTTC,
                    MontantRegle = f.MontantRegle,
                    ResteAPayer = f.MontantTTC - f.MontantRegle,
                    JoursRetard = joursRetard,
                    CategorieRetard = GetCategorieRetard(joursRetard)
                };
            })
            .OrderByDescending(f => f.JoursRetard)
            .ToList();
    }

    private static string GetCategorieRetard(int jours)
    {
        return jours switch
        {
            <= 30 => "1-30 jours",
            <= 60 => "31-60 jours",
            <= 90 => "61-90 jours",
            _ => "Plus de 90 jours"
        };
    }
}
