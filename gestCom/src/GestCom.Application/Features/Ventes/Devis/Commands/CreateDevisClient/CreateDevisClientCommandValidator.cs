using FluentValidation;
using GestCom.Application.Features.Ventes.Devis.DTOs;

namespace GestCom.Application.Features.Ventes.Devis.Commands.CreateDevisClient;

public class CreateDevisClientCommandValidator : AbstractValidator<CreateDevisClientCommand>
{
    public CreateDevisClientCommandValidator()
    {
        RuleFor(x => x.CodeClient)
            .NotEmpty().WithMessage("Le code client est obligatoire.");

        RuleFor(x => x.DateDevis)
            .NotEmpty().WithMessage("La date de devis est obligatoire.");

        RuleFor(x => x.DateValidite)
            .GreaterThanOrEqualTo(x => x.DateDevis)
            .When(x => x.DateValidite.HasValue)
            .WithMessage("La date de validité doit être postérieure ou égale à la date du devis.");

        RuleFor(x => x.TauxRemise)
            .GreaterThanOrEqualTo(0).WithMessage("Le taux de remise doit être positif.")
            .LessThanOrEqualTo(100).WithMessage("Le taux de remise ne peut pas dépasser 100%.");

        RuleFor(x => x.Timbre)
            .GreaterThanOrEqualTo(0).WithMessage("Le timbre doit être positif ou nul.");

        RuleFor(x => x.TauxChange)
            .GreaterThan(0).WithMessage("Le taux de change doit être supérieur à zéro.");

        RuleFor(x => x.Lignes)
            .NotEmpty().WithMessage("Le devis doit contenir au moins une ligne.");

        RuleForEach(x => x.Lignes).SetValidator(new CreateLigneDevisClientDtoValidator());
    }
}

public class CreateLigneDevisClientDtoValidator : AbstractValidator<CreateLigneDevisClientDto>
{
    public CreateLigneDevisClientDtoValidator()
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
