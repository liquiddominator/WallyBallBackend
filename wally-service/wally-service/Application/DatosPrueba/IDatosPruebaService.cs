namespace WallyBallBackend.Application.DatosPrueba;

public interface IDatosPruebaService
{
    Task<DatosPruebaOperationResult> GenerarDatosPruebaAsync(
        GenerarDatosPruebaRequest request,
        CancellationToken cancellationToken);
}

