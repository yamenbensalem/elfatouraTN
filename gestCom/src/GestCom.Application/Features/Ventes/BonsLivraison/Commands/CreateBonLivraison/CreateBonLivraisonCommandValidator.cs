using FluentValidation;
using GestCom.Application.Features.Ventes.BonsLivraison.DTOs;

namespace GestCom.Application.Features.Ventes.BonsLivraison.Commands.CreateBonLivraison;

public class CreateBonLivraisonCommandValidator : AbstractValidator<CreateBonLivraisonCommand>
{
    public CreateBonLivraisonCommandValidator()
    {
        RuleFor(x => x.CodeClient)
            .NotEmpty().WithMessage("Le code client est obligatoire.");

        RuleFor(x => x.DateBonLivraison)
            .NotEmpty().WithMessage("La date du bon de livraison est obligatoire.")
            .LessThanOrEqualTo(DateTime.Now.AddDays(1)).WithMessage("La date du bon de livraison ne peut pas être dans le futur.");

        RuleFor(x => x.Lignes)
            .NotEmpty().WithMessage("Le bon de livraison doit contenir au moins une ligne.");

        RuleForEach(x => x.Lignes)
            .SetValidator(new CreateLigneBonLivraisonDtoValidator());
    }
}

public class CreateLigneBonLivraisonDtoValidator : AbstractValidator<CreateLigneBonLivraisonDto>
{
    public CreateLigneBonLivraisonDtoValidator()
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
    }
}