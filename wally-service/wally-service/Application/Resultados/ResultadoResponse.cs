namespace WallyBallBackend.Application.Resultados;

public sealed record ResultadoResponse(
    int IdResultado,
    int IdPartido,
    int IdCampeonatoCategoria,
    int IdFase,
    string Fase,
    int IdJornada,
    int NumeroJornada,
    int IdEquipoLocal,
    string EquipoLocal,
    int IdEquipoVisitante,
    string EquipoVisitante,
    int SetsLocal,
    int SetsVisitante,
    int IdEquipoGanador,
    string EquipoGanador,
    DateTime FechaRegistro,
    DateTime? FechaActualizacion,
    IReadOnlyCollection<ResultadoSetResponse> Sets);
