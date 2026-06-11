namespace WallyBallBackend.Application.Resultados;

public sealed record UpdateResultadoRequest(
    IReadOnlyCollection<ResultadoSetRequest> Sets,
    string? Motivo);

