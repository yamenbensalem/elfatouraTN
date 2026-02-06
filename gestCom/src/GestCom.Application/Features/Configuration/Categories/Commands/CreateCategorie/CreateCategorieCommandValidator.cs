using FluentValidation;

namespace GestCom.Application.Features.Configuration.Categories.Commands.CreateCategorie;

public class CreateCategorieCommandValidator : AbstractValidator<CreateCategorieCommand>
{
    public CreateCategorieCommandValidator()
    {
        RuleFor(x => x.CodeCategorie)
            .NotEmpty().WithMessage("Le code catégorie est obligatoire.")
            .MaximumLength(20).WithMessage("Le code catégorie ne doit pas dépasser 20 caractères.");

        RuleFor(x => x.LibelleCategorie)
            .MaximumLength(100).WithMessage("Le libellé ne doit pas dépasser 100 caractères.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La description ne doit pas dépasser 500 caractères.");
    }
}
