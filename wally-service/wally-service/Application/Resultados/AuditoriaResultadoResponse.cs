namespace WallyBallBackend.Application.Resultados;

public sealed record AuditoriaResultadoResponse(
    int IdAuditoriaResultado,
    int IdResultado,
    int IdPartido,
    int SetsLocalAnterior,
    int SetsVisitanteAnterior,
    int IdEquipoGanadorAnterior,
    int SetsLocalNuevo,
    int SetsVisitanteNuevo,
    int IdEquipoGanadorNuevo,
    string? Motivo,
    DateTime FechaCambio);
