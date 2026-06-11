using FluentValidation;

namespace PersonasService.Application.Personas;

public sealed class CreateJugadorPersonaRequestValidator : AbstractValidator<CreateJugadorPersonaRequest>
{
    public CreateJugadorPersonaRequestValidator()
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

        RuleFor(request => request.Telefono)
            .MaximumLength(30);

        RuleFor(request => request.FechaNacimiento)
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(request => request.FechaNacimiento.HasValue);

        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(150);

        RuleFor(request => request.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100);
    }
}
