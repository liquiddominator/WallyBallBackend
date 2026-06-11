using FluentValidation;

namespace WallyBallBackend.Application.Resultados;

public sealed class ResultadoSetRequestValidator : AbstractValidator<ResultadoSetRequest>
{
    public ResultadoSetRequestValidator()
    {
        RuleFor(set => set.NumeroSet)
            .GreaterThan(0);

        RuleFor(set => set.PuntosLocal)
            .GreaterThanOrEqualTo(0);

        RuleFor(set => set.PuntosVisitante)
            .GreaterThanOrEqualTo(0);

        RuleFor(set => set)
            .Must(set => set.PuntosLocal != set.PuntosVisitante)
            .WithMessage("Un set no puede terminar empatado.");
    }
}

