using FluentValidation;
using GestCom.Application.Features.Ventes.Factures.DTOs;

namespace GestCom.Application.Features.Ventes.Factures.Commands.CreateFactureClient;

public class CreateFactureClientCommandValidator : AbstractValidator<CreateFactureClientCommand>
{
    public CreateFactureClientCommandValidator()
    {
        RuleFor(x => x.CodeClient)
            .NotEmpty().WithMessage("Le code client est obligatoire.");

        RuleFor(x => x.DateFacture)
            .NotEmpty().WithMessage("La date de facture est obligatoire.")
            .LessThanOrEqualTo(DateTime.Now.AddDays(1)).WithMessage("La date de facture ne peut pas être dans le futur.");

        RuleFor(x => x.DateEcheance)
            .GreaterThanOrEqualTo(x => x.DateFacture)
            .When(x => x.DateEcheance.HasValue)
            .WithMessage("La date d'échéance doit être postérieure ou égale à la date de facture.");

        RuleFor(x => x.TauxRemise)
            .GreaterThanOrEqualTo(0).WithMessage("Le taux de remise doit être positif.")
            .LessThanOrEqualTo(100).WithMessage("Le taux de remise ne peut pas dépasser 100%.");

        RuleFor(x => x.Timbre)
            .GreaterThanOrEqualTo(0).WithMessage("Le timbre doit être positif ou nul.");

        RuleFor(x => x.TauxRAS)
            .GreaterThanOrEqualTo(0).WithMessage("Le taux RAS doit être positif.")
            .LessThanOrEqualTo(100).WithMessage("Le taux RAS ne peut pas dépasser 100%.");

        RuleFor(x => x.TauxChange)
            .GreaterThan(0).WithMessage("Le taux de change doit être supérieur à zéro.");

        RuleFor(x => x.Lignes)
            .NotEmpty().WithMessage("La facture doit contenir au moins une ligne.");

        RuleForEach(x => x.Lignes).SetValidator(new CreateLigneFactureClientDtoValidator());
    }
}

public class CreateLigneFactureClientDtoValidator : AbstractValidator<CreateLigneFactureClientDto>
{
    public CreateLigneFactureClientDtoValidator()
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

        RuleFor(x => x.TauxFODEC)
            .GreaterThanOrEqualTo(0).WithMessage("Le taux FODEC doit être positif.")
            .LessThanOrEqualTo(100).WithMessage("Le taux FODEC ne peut pas dépasser 100%.");
    }
}
