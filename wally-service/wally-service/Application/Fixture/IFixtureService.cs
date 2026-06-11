namespace WallyBallBackend.Application.Fixture;

public interface IFixtureService
{
    Task<FixtureResponse?> GetFixtureAsync(int campeonatoCategoriaId, CancellationToken cancellationToken);

    Task<FixtureOperationResult> GenerateFixtureAsync(
        int campeonatoCategoriaId,
        GenerateFixtureRequest request,
        CancellationToken cancellationToken);

    Task<FixtureOperationResult> ReprogramarPartidoAsync(
        int partidoId,
        ReprogramarPartidoRequest request,
        CancellationToken cancellationToken);
}
