using FluentValidation;

namespace WallyBallBackend.Application.Fixture;

public sealed class GenerateFixtureRequestValidator : AbstractValidator<GenerateFixtureRequest>
{
    public GenerateFixtureRequestValidator()
    {
        RuleFor(request => request.DiasEntreJornadas)
            .InclusiveBetween(1, 30);
    }
}
