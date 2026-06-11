using FluentValidation;

namespace WallyBallBackend.Application.Campeonatos;

public sealed class UpdateCampeonatoRequestValidator : AbstractValidator<UpdateCampeonatoRequest>
{
    public UpdateCampeonatoRequestValidator()
    {
        RuleFor(request => request.Nombre)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(request => request.FechaInicio)
            .NotEqual(default(DateOnly))
            .WithMessage("La fecha de inicio es obligatoria.");

        RuleFor(request => request.FechaFin)
            .NotEqual(default(DateOnly))
            .WithMessage("La fecha de finalizacion es obligatoria.")
            .GreaterThanOrEqualTo(request => request.FechaInicio)
            .WithMessage("La fecha de finalizacion debe ser mayor o igual a la fecha de inicio.");
    }
}
