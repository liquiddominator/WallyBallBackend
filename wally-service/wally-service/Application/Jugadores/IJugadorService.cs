namespace WallyBallBackend.Application.Jugadores;

public interface IJugadorService
{
    Task<IReadOnlyCollection<JugadorResponse>> GetJugadoresAsync(
        string? termino,
        string? cedula,
        int? equipoId,
        CancellationToken cancellationToken);

    Task<JugadorResponse?> GetJugadorByIdAsync(int jugadorId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<JugadorEquipoResponse>> GetJugadoresByEquipoAsync(
        int equipoId,
        CancellationToken cancellationToken);

    Task<JugadorOperationResult> CreateJugadorAsync(
        CreateJugadorRequest request,
        CancellationToken cancellationToken);

    Task<JugadorOperationResult> AsignarJugadorEquipoAsync(
        int equipoId,
        AsignarJugadorEquipoRequest request,
        CancellationToken cancellationToken);
}
