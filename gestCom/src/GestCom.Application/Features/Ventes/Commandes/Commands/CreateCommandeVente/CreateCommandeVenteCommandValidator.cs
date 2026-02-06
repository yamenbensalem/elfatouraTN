using FluentValidation;
using GestCom.Application.Features.Ventes.Commandes.DTOs;

namespace GestCom.Application.Features.Ventes.Commandes.Commands.CreateCommandeVente;

public class CreateCommandeVenteCommandValidator : AbstractValidator<CreateCommandeVenteCommand>
{
    public CreateCommandeVenteCommandValidator()
    {
        RuleFor(x => x.CodeClient)
            .NotEmpty().WithMessage("Le code client est obligatoire.");

        RuleFor(x => x.DateCommande)
            .NotEmpty().WithMessage("La date de commande est obligatoire.");

        RuleFor(x => x.DateLivraisonPrevue)
            .GreaterThanOrEqualTo(x => x.DateCommande)
            .When(x => x.DateLivraisonPrevue.HasValue)
            .WithMessage("La date de livraison prévue doit être postérieure ou égale à la date de commande.");

        RuleFor(x => x.TauxRemise)
            .GreaterThanOrEqualTo(0).WithMessage("Le taux de remise doit être positif.")
            .LessThanOrEqualTo(100).WithMessage("Le taux de remise ne peut pas dépasser 100%.");

        RuleFor(x => x.TauxChange)
            .GreaterThan(0).WithMessage("Le taux de change doit être supérieur à zéro.");

        RuleFor(x => x.Lignes)
            .NotEmpty().WithMessage("La commande doit contenir au moins une ligne.");

        RuleForEach(x => x.Lignes).SetValidator(new CreateLigneCommandeVenteDtoValidator());
    }
}

public class CreateLigneCommandeVenteDtoValidator : AbstractValidator<CreateLigneCommandeVenteDto>
{
    public CreateLigneCommandeVenteDtoValidator()
    {
        RuleFor(x => x.CodeProduit)
            .NotEmpty().WithMessage("Le code produit est obligatoire pour chaque ligne.");

        RuleFor(x => x.Quantite)
            .GreaterThan(0).WithMessage("La quantité doit être supérieure à zéro.");

        RuleFor(x => x.PrixUnitaireHT)
            .GreaterThanOrEqualTo(0).WithMessage("Le prix unitaire HT doit être positif ou nul.");

        RuleFor(x => x.TauxTVA)
            .GreaterThanOrEqualTo(0).WithMessage("Le taux TVA doit être positif.")
            .LessThanOrEqualTo(100).WithMessage("Le taux TVA ne peut pas dépasser 100%.");

        RuleFor(x => x.TauxRemise)
            .GreaterThanOrEqualTo(0).WithMessage("Le taux de remise doit être positif.")
            .LessThanOrEqualTo(100).WithMessage("Le taux de remise ne peut pas dépasser 100%.");
    }
}
