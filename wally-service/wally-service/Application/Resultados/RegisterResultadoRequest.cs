namespace WallyBallBackend.Application.Resultados;

public sealed record RegisterResultadoRequest(
    IReadOnlyCollection<ResultadoSetRequest> Sets);

