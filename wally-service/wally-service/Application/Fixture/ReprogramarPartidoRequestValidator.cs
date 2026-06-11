using FluentValidation;

namespace WallyBallBackend.Application.Fixture;

public sealed class ReprogramarPartidoRequestValidator : AbstractValidator<ReprogramarPartidoRequest>
{
    public ReprogramarPartidoRequestValidator()
    {
        RuleFor(request => request.FechaHoraNueva)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("La nueva fecha y hora debe ser posterior a la fecha actual.");

        RuleFor(request => request.Motivo)
            .MaximumLength(300);
    }
}
