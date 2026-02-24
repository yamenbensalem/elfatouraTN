using System.Globalization;
using GestCom.Application.Features.Reporting.DTOs;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Reporting.Queries.GetDashboard;

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDashboardQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var dateRef = request.DateReference ?? DateTime.Today;
        var debutMois = new DateTime(dateRef.Year, dateRef.Month, 1);
        var finMois = debutMois.AddMonths(1).AddDays(-1);
        var debutMoisPrecedent = debutMois.AddMonths(-1);
        var finMoisPrecedent = debutMois.AddDays(-1);
        var debutAnnee = new DateTime(dateRef.Year, 1, 1);

        var dashboard = new DashboardDto();

        // Récupérer toutes les factures clients
        var facturesClient = await _unitOfWork.FacturesClient.GetAllAsync();
        var facturesClientList = facturesClient.ToList();

        // Chiffre d'affaires du mois
        var facturesMois = facturesClientList
            .Where(f => f.DateFacture >= debutMois && f.DateFacture <= finMois)
            .ToList();
        dashboard.ChiffreAffairesMois = facturesMois.Sum(f => f.MontantTTC);

        // Chiffre d'affaires du mois précédent
        var facturesMoisPrec = facturesClientList
            .Where(f => f.DateFacture >= debutMoisPrecedent && f.DateFacture <= finMoisPrecedent)
            .ToList();
        dashboard.ChiffreAffairesMoisPrecedent = facturesMoisPrec.Sum(f => f.MontantTTC);

        // Evolution CA
        if (dashboard.ChiffreAffairesMoisPrecedent > 0)
        {
            dashboard.EvolutionCA = ((dashboard.ChiffreAffairesMois - dashboard.ChiffreAffairesMoisPrecedent) 
                / dashboard.ChiffreAffairesMoisPrecedent) * 100;
        }

        // Chiffre d'affaires année
        var facturesAnnee = facturesClientList
            .Where(f => f.DateFacture >= debutAnnee && f.DateFacture.Year == dateRef.Year)
            .ToList();
        dashboard.ChiffreAffairesAnnee = facturesAnnee.Sum(f => f.MontantTTC);

        // Créances (factures non payées)
        var facturesImpayees = facturesClientList
            .Where(f => f.Statut != "Payée")
            .ToList();
        dashboard.FacturesImpayees = facturesImpayees.Count;
        dashboard.TotalCreances = facturesImpayees.Sum(f => f.MontantTTC - f.MontantRegle);

        // Achats
        var facturesFournisseur = await _unitOfWork.FacturesFournisseur.GetAllAsync();
        var facturesFournisseurList = facturesFournisseur.ToList();
        
        dashboard.TotalAchatsMois = facturesFournisseurList
            .Where(f => f.DateFacture >= debutMois && f.DateFacture <= finMois)
            .Sum(f => f.MontantTTC);
        
        var facturesFournisseurImpayees = facturesFournisseurList
            .Where(f => f.Statut != "Payée")
            .ToList();
        dashboard.FacturesFournisseurImpayees = facturesFournisseurImpayees.Count;
        dashboard.TotalDettes = facturesFournisseurImpayees.Sum(f => f.MontantTTC - f.MontantRegle);

        // Stock
        var produits = await _unitOfWork.Produits.GetAllAsync();
        var produitsList = produits.ToList();
        
        dashboard.ProduitsEnStock = produitsList.Count(p => p.Quantite > 0);
        dashboard.ProduitsStockFaible = produitsList.Count(p => p.Quantite > 0 && p.Quantite <= p.StockMinimal);
        dashboard.ValeurStock = produitsList.Sum(p => p.Quantite * p.PrixAchatTTC);

        // Documents du jour
        var aujourdhui = dateRef.Date;
        
        var devis = await _unitOfWork.DevisClients.GetAllAsync();
        dashboard.DevisAujourdhui = devis.Count(d => d.DateDevis.Date == aujourdhui);
        
        var commandes = await _unitOfWork.CommandesVente.GetAllAsync();
        dashboard.CommandesAujourdhui = commandes.Count(c => c.DateCommande.Date == aujourdhui);
        
        dashboard.FacturesAujourdhui = facturesClientList.Count(f => f.DateFacture.Date == aujourdhui);
        
        var bonsLivraison = await _unitOfWork.BonsLivraison.GetAllAsync();
        dashboard.BonsLivraisonAujourdhui = bonsLivraison.Count(b => b.DateBonLivraison.Date == aujourdhui);

        // Top clients
        var clients = await _unitOfWork.Clients.GetAllAsync();
        var clientsDict = clients.ToDictionary(c => c.CodeClient, c => c.NomClient);
        
        dashboard.TopClients = facturesClientList
            .GroupBy(f => f.CodeClient)
            .Select(g => new TopClientDto
            {
                CodeClient = g.Key,
                NomClient = clientsDict.GetValueOrDefault(g.Key),
                ChiffreAffaires = g.Sum(f => f.MontantTTC),
                NombreFactures = g.Count()
            })
            .OrderByDescending(c => c.ChiffreAffaires)
            .Take(request.NombreTopClients)
            .ToList();

        // Top produits (basé sur les lignes de factures)
        var lignesFactures = facturesClientList
            .SelectMany(f => f.Lignes)
            .ToList();
        
        var produitsDict = produitsList.ToDictionary(p => p.CodeProduit, p => p.Designation);
        
        dashboard.TopProduits = lignesFactures
            .GroupBy(l => l.CodeProduit)
            .Select(g => new TopProduitDto
            {
                CodeProduit = g.Key,
                Designation = produitsDict.GetValueOrDefault(g.Key),
                QuantiteVendue = g.Sum(l => l.Quantite),
                ChiffreAffaires = g.Sum(l => l.MontantTTC)
            })
            .OrderByDescending(p => p.ChiffreAffaires)
            .Take(request.NombreTopProduits)
            .ToList();

        // CA par mois (12 derniers mois)
        var debutPeriode = debutMois.AddMonths(-11);
        var culture = new CultureInfo("fr-FR");
        
        dashboard.CAParMois = Enumerable.Range(0, 12)
            .Select(i =>
            {
                var mois = debutPeriode.AddMonths(i);
                var finMoisCourant = mois.AddMonths(1).AddDays(-1);
                var facturesDuMois = facturesClientList
                    .Where(f => f.DateFacture >= mois && f.DateFacture <= finMoisCourant)
                    .ToList();
                
                return new ChiffreAffairesParMoisDto
                {
                    Annee = mois.Year,
                    Mois = mois.Month,
                    NomMois = culture.DateTimeFormat.GetMonthName(mois.Month),
                    MontantHT = facturesDuMois.Sum(f => f.MontantHT),
                    MontantTTC = facturesDuMois.Sum(f => f.MontantTTC)
                };
            })
            .ToList();

        // Ventes par catégorie
        var categories = await _unitOfWork.CategoriesProduit.GetAllAsync();
        var categoriesDict = categories.ToDictionary(c => c.CodeCategorie.ToString(), c => c.LibelleCategorie);
        var produitsCategories = produitsList.ToDictionary(p => p.CodeProduit, p => p.CodeCategorie);
        
        var ventesParCategorie = lignesFactures
            .Where(l => produitsCategories.ContainsKey(l.CodeProduit))
            .GroupBy(l => produitsCategories[l.CodeProduit])
            .Select(g => new VentesParCategorieDto
            {
                CodeCategorie = g.Key ?? string.Empty,
                LibelleCategorie = g.Key != null ? categoriesDict.GetValueOrDefault(g.Key, string.Empty) : null,
                MontantVentes = g.Sum(l => l.MontantTTC)
            })
            .OrderByDescending(v => v.MontantVentes)
            .ToList();

        var totalVentes = ventesParCategorie.Sum(v => v.MontantVentes);
        if (totalVentes > 0)
        {
            foreach (var vente in ventesParCategorie)
            {
                vente.Pourcentage = (vente.MontantVentes / totalVentes) * 100;
            }
        }
        
        dashboard.VentesParCategorie = ventesParCategorie;

        return dashboard;
    }
}
