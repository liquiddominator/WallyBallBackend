namespace WallyBallBackend.Application.Categorias;

public interface ICategoriaService
{
    Task<IReadOnlyCollection<CategoriaResponse>> GetCategoriasAsync(int? campeonatoId, CancellationToken cancellationToken);

    Task<CategoriaResponse?> GetCategoriaByIdAsync(int categoriaId, CancellationToken cancellationToken);

    Task<CategoriaOperationResult> CreateCategoriaAsync(CreateCategoriaRequest request, CancellationToken cancellationToken);

    Task<CategoriaOperationResult> AddCategoriaToCampeonatoAsync(
        int campeonatoId,
        AddCategoriaCampeonatoRequest request,
        CancellationToken cancellationToken);
}
