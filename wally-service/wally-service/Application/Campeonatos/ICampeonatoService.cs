namespace WallyBallBackend.Application.Campeonatos;

public interface ICampeonatoService
{
    Task<IReadOnlyCollection<CampeonatoResponse>> GetCampeonatosAsync(CancellationToken cancellationToken);

    Task<CampeonatoResponse?> GetCampeonatoByIdAsync(int campeonatoId, CancellationToken cancellationToken);

    Task<CampeonatoOperationResult> CreateCampeonatoAsync(CreateCampeonatoRequest request, CancellationToken cancellationToken);

    Task<CampeonatoOperationResult> UpdateCampeonatoAsync(int campeonatoId, UpdateCampeonatoRequest request, CancellationToken cancellationToken);

    Task<CampeonatoOperationResult> FinalizeCampeonatoAsync(int campeonatoId, CancellationToken cancellationToken);
}
