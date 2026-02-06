using FluentValidation;

namespace GestCom.Application.Features.Achats.Fournisseurs.Commands.CreateFournisseur;

public class CreateFournisseurCommandValidator : AbstractValidator<CreateFournisseurCommand>
{
    public CreateFournisseurCommandValidator()
    {
        RuleFor(x => x.CodeFournisseur)
            .NotEmpty().WithMessage("Le code fournisseur est obligatoire.")
            .MaximumLength(50).WithMessage("Le code fournisseur ne doit pas dépasser 50 caractères.");

        RuleFor(x => x.RaisonSociale)
            .NotEmpty().WithMessage("La raison sociale est obligatoire.")
            .MaximumLength(200).WithMessage("La raison sociale ne doit pas dépasser 200 caractères.");

        RuleFor(x => x.MatriculeFiscale)
            .Matches(@"^\d{7}[A-Z]{3}\d{3}$")
            .When(x => !string.IsNullOrEmpty(x.MatriculeFiscale))
            .WithMessage("Le matricule fiscal doit être au format tunisien (7 chiffres + 3 lettres + 3 chiffres).");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("L'adresse email n'est pas valide.");

        RuleFor(x => x.Telephone)
            .MaximumLength(20).WithMessage("Le téléphone ne doit pas dépasser 20 caractères.");

        RuleFor(x => x.DelaiPaiement)
            .GreaterThanOrEqualTo(0).WithMessage("Le délai de paiement doit être positif ou nul.");

        RuleFor(x => x.TauxRemise)
            .GreaterThanOrEqualTo(0).WithMessage("Le taux de remise doit être positif ou nul.")
            .LessThanOrEqualTo(100).WithMessage("Le taux de remise ne peut pas dépasser 100%.");
    }
}
