using GestCom.Domain.Interfaces;
using MediatR;

namespace GestCom.Application.Features.Ventes.Produits.Queries.GetProduitsStockFaible;

public class GetProduitsStockFaibleQueryHandler : IRequestHandler<GetProduitsStockFaibleQuery, IEnumerable<ProduitStockAlertDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProduitsStockFaibleQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ProduitStockAlertDto>> Handle(GetProduitsStockFaibleQuery request, CancellationToken cancellationToken)
    {
        var produits = await _unitOfWork.Produits.GetAllAsync();
        var produitsList = produits.ToList();

        // Filtrer par catégorie
        if (!string.IsNullOrEmpty(request.CodeCategorie))
        {
            produitsList = produitsList.Where(p => p.CodeCategorie == request.CodeCategorie).ToList();
        }

        // Filtrer les produits avec stock faible ou en rupture
        var produitsAlertes = produitsList
            .Where(p => p.StockMinimal > 0 && p.Quantite <= p.StockMinimal)
            .ToList();

        if (!request.InclureRupture)
        {
            produitsAlertes = produitsAlertes.Where(p => p.Quantite > 0).ToList();
        }

        // Récupérer les catégories
        var categories = await _unitOfWork.CategoriesProduit.GetAllAsync();
        var categoriesDict = categories.ToDictionary(c => c.CodeCategorie.ToString(), c => c.LibelleCategorie);

        // Récupérer les fournisseurs
        var fournisseurs = await _unitOfWork.Fournisseurs.GetAllAsync();
        var fournisseursDict = fournisseurs.ToDictionary(f => f.CodeFournisseur, f => f.NomFournisseur);

        return produitsAlertes
            .Select(p => new ProduitStockAlertDto
            {
                CodeProduit = p.CodeProduit,
                CodeBarre = p.CodeBarre,
                Designation = p.Designation,
                LibelleCategorie = p.CodeCategorie != null ? categoriesDict.GetValueOrDefault(p.CodeCategorie, string.Empty) : null,
                NomFournisseur = p.CodeFournisseur != null ? fournisseursDict.GetValueOrDefault(p.CodeFournisseur, string.Empty) : null,
                Quantite = p.Quantite,
                StockMinimal = p.StockMinimal,
                QuantiteACommander = p.StockMinimal - p.Quantite + (p.StockMinimal * 0.2m), // Stock minimal + 20% de marge
                Niveau = p.Quantite <= 0 ? "Rupture" : 
                        p.Quantite <= p.StockMinimal * 0.5m ? "Critique" : "Faible",
                PrixAchatTTC = p.PrixAchatTTC
            })
            .OrderBy(p => p.Niveau == "Rupture" ? 0 : p.Niveau == "Critique" ? 1 : 2)
            .ThenBy(p => p.Quantite / p.StockMinimal)
            .ToList();
    }
}
