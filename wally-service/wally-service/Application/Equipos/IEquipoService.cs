namespace WallyBallBackend.Application.Equipos;

public interface IEquipoService
{
    Task<IReadOnlyCollection<EquipoResponse>> GetEquiposAsync(int? campeonatoCategoriaId, CancellationToken cancellationToken);

    Task<EquipoResponse?> GetEquipoByIdAsync(int equipoId, CancellationToken cancellationToken);

    Task<EquipoOperationResult> CreateEquipoAsync(
        int campeonatoCategoriaId,
        CreateEquipoRequest request,
        CancellationToken cancellationToken);

    Task<EquipoOperationResult> UpdateEquipoAsync(
        int equipoId,
        UpdateEquipoRequest request,
        CancellationToken cancellationToken);
}
