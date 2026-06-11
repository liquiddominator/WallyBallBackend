using FluentValidation;

namespace PersonasService.Application.Auth;

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(request => request.CurrentPassword)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100)
            .NotEqual(request => request.CurrentPassword)
            .WithMessage("La nueva contrasena debe ser diferente a la actual.");
    }
}
