using WallyBallBackend.Application.Resultados;

namespace WallyBallBackend.Application.Reportes;

public sealed record ReporteResultadoResponse(
    int IdResultado,
    int IdPartido,
    int IdCampeonatoCategoria,
    int IdCampeonato,
    string Campeonato,
    int IdCategoria,
    string Categoria,
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
