using FluentValidation;

namespace IdentidadService.Application.Auth;

public sealed class LogoutRequestValidator : AbstractValidator<LogoutRequest>
{
    public LogoutRequestValidator()
    {
        RuleFor(request => request.RefreshToken)
            .MaximumLength(500);
    }
}
