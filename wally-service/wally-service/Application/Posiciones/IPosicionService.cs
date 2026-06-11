namespace WallyBallBackend.Application.Posiciones;

public interface IPosicionService
{
    Task<IReadOnlyCollection<PosicionResponse>?> GetTablaPosicionesAsync(
        int campeonatoCategoriaId,
        CancellationToken cancellationToken);
}

