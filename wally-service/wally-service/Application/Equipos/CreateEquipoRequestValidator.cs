using FluentValidation;

namespace WallyBallBackend.Application.Equipos;

public sealed class CreateEquipoRequestValidator : AbstractValidator<CreateEquipoRequest>
{
    public CreateEquipoRequestValidator()
    {
        RuleFor(request => request.Nombre)
            .NotEmpty()
            .MaximumLength(120);
    }
}
