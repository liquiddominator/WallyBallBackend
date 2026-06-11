using FluentValidation;

namespace WallyBallBackend.Application.Jugadores;

public sealed class CreateJugadorRequestValidator : AbstractValidator<CreateJugadorRequest>
{
    public CreateJugadorRequestValidator()
    {
        RuleFor(request => request.Cedula)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(request => request.Nombre)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Apellido)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(150);

        RuleFor(request => request.PasswordTemporal)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100);

        RuleFor(request => request.Telefono)
            .MaximumLength(30);

        RuleFor(request => request.FechaNacimiento)
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(request => request.FechaNacimiento.HasValue)
            .WithMessage("La fecha de nacimiento debe ser anterior a la fecha actual.");
    }
}
