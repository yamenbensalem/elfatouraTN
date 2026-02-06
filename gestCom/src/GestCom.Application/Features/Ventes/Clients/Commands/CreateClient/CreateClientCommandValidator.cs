using FluentValidation;

namespace GestCom.Application.Features.Ventes.Clients.Commands.CreateClient;

/// <summary>
/// Validateur pour la création de client
/// </summary>
public class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.CodeClient)
            .NotEmpty().WithMessage("Le code client est obligatoire.")
            .MaximumLength(50).WithMessage("Le code client ne peut pas dépasser 50 caractères.");

        RuleFor(x => x.Nom)
            .NotEmpty().WithMessage("Le nom du client est obligatoire.")
            .MaximumLength(200).WithMessage("Le nom ne peut pas dépasser 200 caractères.");

        RuleFor(x => x.MatriculeFiscale)
            .NotEmpty().WithMessage("Le matricule fiscal est obligatoire.")
            .MaximumLength(50).WithMessage("Le matricule fiscal ne peut pas dépasser 50 caractères.")
            .Matches(@"^\d{7}[A-Z]{3}\d{3}$")
            .When(x => !string.IsNullOrEmpty(x.MatriculeFiscale))
            .WithMessage("Format de matricule fiscal invalide (ex: 1234567ABC123).");

        RuleFor(x => x.TypePersonne)
            .NotEmpty().WithMessage("Le type de personne est obligatoire.")
            .Must(x => x == "Personne Physique" || x == "Personne Morale")
            .WithMessage("Le type de personne doit être 'Personne Physique' ou 'Personne Morale'.");

        RuleFor(x => x.Adresse)
            .NotEmpty().WithMessage("L'adresse est obligatoire.")
            .MaximumLength(500).WithMessage("L'adresse ne peut pas dépasser 500 caractères.");

        RuleFor(x => x.Ville)
            .NotEmpty().WithMessage("La ville est obligatoire.")
            .MaximumLength(100).WithMessage("La ville ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("L'adresse email n'est pas valide.");

        RuleFor(x => x.Tel)
            .Matches(@"^[\d\s\+\-\(\)]{8,20}$")
            .When(x => !string.IsNullOrEmpty(x.Tel))
            .WithMessage("Le numéro de téléphone n'est pas valide.");

        RuleFor(x => x.MaxCredit)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Le crédit maximum ne peut pas être négatif.");

        RuleFor(x => x.CodeDevise)
            .GreaterThan(0)
            .WithMessage("La devise est obligatoire.");
    }
}
