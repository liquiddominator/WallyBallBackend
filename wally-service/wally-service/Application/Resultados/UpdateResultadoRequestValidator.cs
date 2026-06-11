using FluentValidation;

namespace WallyBallBackend.Application.Resultados;

public sealed class UpdateResultadoRequestValidator : AbstractValidator<UpdateResultadoRequest>
{
    public UpdateResultadoRequestValidator()
    {
        RuleFor(request => request.Sets)
            .NotNull()
            .NotEmpty();

        RuleForEach(request => request.Sets)
            .SetValidator(new ResultadoSetRequestValidator());

        RuleFor(request => request.Sets)
            .Must(HaveUniqueSetNumbers)
            .WithMessage("No puede repetirse el numero de set.")
            .Must(HaveWinner)
            .WithMessage("El resultado consolidado no puede terminar empatado en sets.");

        RuleFor(request => request.Motivo)
            .MaximumLength(300);
    }

    private static bool HaveUniqueSetNumbers(IReadOnlyCollection<ResultadoSetRequest>? sets)
    {
        return sets is null || sets.Select(set => set.NumeroSet).Distinct().Count() == sets.Count;
    }

    private static bool HaveWinner(IReadOnlyCollection<ResultadoSetRequest>? sets)
    {
        if (sets is null || sets.Count == 0)
        {
            return true;
        }

        var localSets = sets.Count(set => set.PuntosLocal > set.PuntosVisitante);
        var visitanteSets = sets.Count(set => set.PuntosVisitante > set.PuntosLocal);

        return localSets != visitanteSets;
    }
}

