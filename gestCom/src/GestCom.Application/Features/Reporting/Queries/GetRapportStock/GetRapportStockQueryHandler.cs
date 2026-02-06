using GestCom.Application.Features.Reporting.DTOs;
using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Reporting.Queries.GetRapportStock;

public class GetRapportStockQueryHandler : IRequestHandler<GetRapportStockQuery, RapportStockDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRapportStockQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RapportStockDto> Handle(GetRapportStockQuery request, CancellationToken cancellationToken)
    {
        var rapport = new RapportStockDto();

        // Récupérer les produits
        var produits = await _unitOfWork.Produits.GetAllAsync();
        var produitsList = produits.ToList();

        // Filtrer par catégorie si spécifiée
        if (!string.IsNullOrEmpty(request.CodeCategorie))
        {
            produitsList = produitsList.Where(p => p.CodeCategorie == request.CodeCategorie).ToList();
        }

        // Filtrer par magasin si spécifié
        if (!string.IsNullOrEmpty(request.CodeMagasin))
        {
            produitsList = produitsList.Where(p => p.CodeMagasin == request.CodeMagasin).ToList();
        }

        // Statistiques globales
        rapport.TotalProduits = produitsList.Count;
        rapport.ProduitsEnStock = produitsList.Count(p => p.Quantite > 0);
        rapport.ProduitsRupture = produitsList.Count(p => p.Quantite <= 0);
        rapport.ProduitsStockFaible = produitsList.Count(p => 
            p.Quantite > 0 && 
            p.StockMinimal > 0 && 
            p.Quantite <= p.StockMinimal * request.SeuilStockFaible);
        rapport.ValeurStockTotal = produitsList.Sum(p => p.Quantite * p.PrixAchatTTC);

        // Récupérer les catégories pour les libellés
        var categories = await _unitOfWork.CategoriesProduit.GetAllAsync();
        var categoriesDict = categories.ToDictionary(c => c.CodeCategorie.ToString(), c => c.LibelleCategorie);

        // Produits avec alertes stock (triés par niveau d'urgence)
        rapport.ProduitsAlertes = produitsList
            .Where(p => p.StockMinimal > 0 && p.Quantite <= p.StockMinimal * request.SeuilStockFaible)
            .Select(p => new ProduitStockDto
            {
                CodeProduit = p.CodeProduit,
                Designation = p.Designation,
                LibelleCategorie = p.CodeCategorie != null ? categoriesDict.GetValueOrDefault(p.CodeCategorie, string.Empty) : null,
                Quantite = p.Quantite,
                StockMinimal = p.StockMinimal,
                Ecart = p.StockMinimal - p.Quantite,
                Niveau = p.Quantite <= 0 ? "Rupture" : 
                         p.Quantite <= p.StockMinimal ? "Critique" : "Faible"
            })
            .OrderBy(p => p.Niveau == "Rupture" ? 0 : p.Niveau == "Critique" ? 1 : 2)
            .ThenBy(p => p.Ecart)
            .ToList();

        // Stock par catégorie
        rapport.StockParCategorie = produitsList
            .Where(p => !string.IsNullOrEmpty(p.CodeCategorie))
            .GroupBy(p => p.CodeCategorie!)
            .Select(g => new StockParCategorieDto
            {
                CodeCategorie = g.Key,
                LibelleCategorie = categoriesDict.GetValueOrDefault(g.Key, string.Empty),
                NombreProduits = g.Count(),
                QuantiteTotale = g.Sum(p => p.Quantite),
                ValeurStock = g.Sum(p => p.Quantite * p.PrixAchatTTC)
            })
            .OrderByDescending(s => s.ValeurStock)
            .ToList();

        return rapport;
    }
}
