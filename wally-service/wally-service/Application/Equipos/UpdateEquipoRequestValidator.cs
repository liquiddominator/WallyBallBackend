using FluentValidation;

namespace WallyBallBackend.Application.Equipos;

public sealed class UpdateEquipoRequestValidator : AbstractValidator<UpdateEquipoRequest>
{
    public UpdateEquipoRequestValidator()
    {
        RuleFor(request => request.Nombre)
            .NotEmpty()
            .MaximumLength(120);
    }
}
