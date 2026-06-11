using FluentValidation;

namespace WallyBallBackend.Application.Jugadores;

public sealed class AsignarJugadorEquipoRequestValidator : AbstractValidator<AsignarJugadorEquipoRequest>
{
    public AsignarJugadorEquipoRequestValidator()
    {
        RuleFor(request => request.IdJugador)
            .GreaterThan(0);
    }
}
