using FluentValidation;

namespace GestCom.Application.Features.Ventes.Produits.Commands.CreateProduit;

public class CreateProduitCommandValidator : AbstractValidator<CreateProduitCommand>
{
    public CreateProduitCommandValidator()
    {
        RuleFor(x => x.CodeProduit)
            .NotEmpty().WithMessage("Le code produit est obligatoire.")
            .MaximumLength(50).WithMessage("Le code produit ne doit pas dépasser 50 caractères.");

        RuleFor(x => x.Designation)
            .NotEmpty().WithMessage("La désignation est obligatoire.")
            .MaximumLength(200).WithMessage("La désignation ne doit pas dépasser 200 caractères.");

        RuleFor(x => x.CodeBarre)
            .MaximumLength(50).WithMessage("Le code barre ne doit pas dépasser 50 caractères.")
            .When(x => !string.IsNullOrEmpty(x.CodeBarre));

        RuleFor(x => x.Reference)
            .MaximumLength(100).WithMessage("La référence ne doit pas dépasser 100 caractères.")
            .When(x => !string.IsNullOrEmpty(x.Reference));

        RuleFor(x => x.PrixAchatTTC)
            .GreaterThanOrEqualTo(0).WithMessage("Le prix d'achat TTC doit être positif ou nul.");

        RuleFor(x => x.TauxMarge)
            .GreaterThanOrEqualTo(0).WithMessage("Le taux de marge doit être positif ou nul.")
            .LessThanOrEqualTo(1000).WithMessage("Le taux de marge ne peut pas dépasser 1000%.");

        RuleFor(x => x.PrixVenteHT)
            .GreaterThanOrEqualTo(0).WithMessage("Le prix de vente HT doit être positif ou nul.");

        RuleFor(x => x.PrixVenteTTC)
            .GreaterThanOrEqualTo(0).WithMessage("Le prix de vente TTC doit être positif ou nul.");

        RuleFor(x => x.Quantite)
            .GreaterThanOrEqualTo(0).WithMessage("La quantité doit être positive ou nulle.");

        RuleFor(x => x.StockMinimal)
            .GreaterThanOrEqualTo(0).WithMessage("Le stock minimal doit être positif ou nul.");

        RuleFor(x => x.TauxTVA)
            .GreaterThanOrEqualTo(0).WithMessage("Le taux TVA doit être positif ou nul.")
            .LessThanOrEqualTo(100).WithMessage("Le taux TVA ne peut pas dépasser 100%.");

        RuleFor(x => x.TauxFODEC)
            .GreaterThanOrEqualTo(0).WithMessage("Le taux FODEC doit être positif ou nul.")
            .LessThanOrEqualTo(100).WithMessage("Le taux FODEC ne peut pas dépasser 100%.");
    }
}
