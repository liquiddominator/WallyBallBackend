using FluentValidation;

namespace WallyBallBackend.Application.Categorias;

public sealed class AddCategoriaCampeonatoRequestValidator : AbstractValidator<AddCategoriaCampeonatoRequest>
{
    public AddCategoriaCampeonatoRequestValidator()
    {
        RuleFor(request => request.IdCategoria)
            .GreaterThan(0);
    }
}
