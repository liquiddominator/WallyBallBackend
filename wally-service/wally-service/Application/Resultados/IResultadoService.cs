namespace WallyBallBackend.Application.Resultados;

public interface IResultadoService
{
    Task<IReadOnlyCollection<ResultadoResponse>> GetResultadosAsync(
        int? campeonatoCategoriaId,
        CancellationToken cancellationToken);

    Task<ResultadoResponse?> GetResultadoByIdAsync(int resultadoId, CancellationToken cancellationToken);

    Task<ResultadoResponse?> GetResultadoByPartidoAsync(int partidoId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<AuditoriaResultadoResponse>> GetAuditoriaResultadoAsync(
        int resultadoId,
        CancellationToken cancellationToken);

    Task<ResultadoOperationResult> RegisterResultadoAsync(
        int partidoId,
        RegisterResultadoRequest request,
        CancellationToken cancellationToken);

    Task<ResultadoOperationResult> UpdateResultadoAsync(
        int resultadoId,
        UpdateResultadoRequest request,
        CancellationToken cancellationToken);
}

