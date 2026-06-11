using FluentValidation;

namespace WallyBallBackend.Application.Categorias;

public sealed class CreateCategoriaRequestValidator : AbstractValidator<CreateCategoriaRequest>
{
    public CreateCategoriaRequestValidator()
    {
        RuleFor(request => request.Nombre)
            .NotEmpty()
            .MaximumLength(80);
    }
}
