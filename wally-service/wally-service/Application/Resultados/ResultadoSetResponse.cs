namespace WallyBallBackend.Application.Resultados;

public sealed record ResultadoSetResponse(
    int IdResultadoSet,
    int NumeroSet,
    int PuntosLocal,
    int PuntosVisitante,
    int IdEquipoGanadorSet,
    string EquipoGanadorSet);
