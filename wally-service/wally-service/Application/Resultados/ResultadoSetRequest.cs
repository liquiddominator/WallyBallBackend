namespace WallyBallBackend.Application.Resultados;

public sealed record ResultadoSetRequest(
    int NumeroSet,
    int PuntosLocal,
    int PuntosVisitante);
