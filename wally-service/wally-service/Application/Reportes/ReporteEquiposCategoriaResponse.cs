namespace WallyBallBackend.Application.Reportes;

public sealed record ReporteEquiposCategoriaResponse(
    int IdCampeonatoCategoria,
    int IdCampeonato,
    string Campeonato,
    int IdCategoria,
    string Categoria,
    int TotalEquipos,
    int TotalJugadoresActivos,
    IReadOnlyCollection<ReporteEquipoResponse> Equipos);

public sealed record ReporteEquipoResponse(
    int IdEquipo,
    string Equipo,
    bool Activo,
    int CantidadJugadoresActivos,
    DateTime FechaCreacion);
