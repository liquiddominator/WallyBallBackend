using FluentValidation;

namespace WallyBallBackend.Application.DatosPrueba;

public sealed class GenerarDatosPruebaRequestValidator : AbstractValidator<GenerarDatosPruebaRequest>
{
    public GenerarDatosPruebaRequestValidator()
    {
        RuleFor(request => request.Categorias)
            .InclusiveBetween(1, 4);

        RuleFor(request => request.EquiposPorCategoria)
            .InclusiveBetween(2, 8);

        RuleFor(request => request.JugadoresPorEquipo)
            .InclusiveBetween(1, 12);
    }
}
