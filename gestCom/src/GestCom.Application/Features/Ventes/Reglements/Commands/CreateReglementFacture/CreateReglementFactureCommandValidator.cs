using FluentValidation;

namespace GestCom.Application.Features.Ventes.Reglements.Commands.CreateReglementFacture;

public class CreateReglementFactureCommandValidator : AbstractValidator<CreateReglementFactureCommand>
{
    public CreateReglementFactureCommandValidator()
    {
        RuleFor(x => x.NumeroFacture)
            .NotEmpty().WithMessage("Le numéro de facture est obligatoire.");

        RuleFor(x => x.DateReglement)
            .NotEmpty().WithMessage("La date du règlement est obligatoire.")
            .LessThanOrEqualTo(DateTime.Now.AddDays(1)).WithMessage("La date du règlement ne peut pas être dans le futur.");

        RuleFor(x => x.Montant)
            .GreaterThan(0).WithMessage("Le montant du règlement doit être supérieur à zéro.");

        RuleFor(x => x.ModePayement)
            .MaximumLength(50).WithMessage("Le mode de paiement ne peut pas dépasser 50 caractères.")
            .When(x => !string.IsNullOrWhiteSpace(x.ModePayement));

        RuleFor(x => x.NumeroTransaction)
            .MaximumLength(100).WithMessage("La référence de transaction ne peut pas dépasser 100 caractères.")
            .When(x => !string.IsNullOrWhiteSpace(x.NumeroTransaction));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Les notes ne peuvent pas dépasser 500 caractères.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}