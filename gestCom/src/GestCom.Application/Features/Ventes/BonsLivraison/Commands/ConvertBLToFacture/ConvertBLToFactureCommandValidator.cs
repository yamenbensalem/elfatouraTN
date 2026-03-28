using FluentValidation;

namespace GestCom.Application.Features.Ventes.BonsLivraison.Commands.ConvertBLToFacture;

public class ConvertBLToFactureCommandValidator : AbstractValidator<ConvertBLToFactureCommand>
{
    public ConvertBLToFactureCommandValidator()
    {
        RuleFor(x => x.NumerosBonLivraison)
            .NotEmpty().WithMessage("Au moins un bon de livraison doit être spécifié.");

        RuleForEach(x => x.NumerosBonLivraison)
            .NotEmpty().WithMessage("Le numéro de bon de livraison ne peut pas être vide.");

        RuleFor(x => x.DateFacture)
            .NotEmpty().WithMessage("La date de facture est obligatoire.")
            .LessThanOrEqualTo(DateTime.Now.AddDays(1)).WithMessage("La date de facture ne peut pas être dans le futur.");

        RuleFor(x => x.DateEcheance)
            .GreaterThanOrEqualTo(x => x.DateFacture)
            .WithMessage("La date d'échéance doit être postérieure ou égale à la date de facture.");

        RuleFor(x => x.TauxRemiseGlobale)
            .GreaterThanOrEqualTo(0).WithMessage("Le taux de remise globale doit être positif.")
            .LessThanOrEqualTo(100).WithMessage("Le taux de remise globale ne peut pas dépasser 100%.");
    }
}