using Cassandra;
using ReportesService.Application.Reportes;
using ReportesService.Infrastructure.Cassandra;
using System.Text.Json;

namespace ReportesService.Infrastructure.Reportes;

public sealed class CassandraReporteQueryService : IReporteQueryService
{
    private readonly ICassandraSessionFactory _sessionFactory;

    public CassandraReporteQueryService(ICassandraSessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    public async Task<IReadOnlyCollection<ReporteEquiposCategoriaResponse>> GetEquiposAsync(
        int? campeonatoId,
        int? campeonatoCategoriaId,
        CancellationToken cancellationToken)
    {
        var session = await _sessionFactory.CreateSessionAsync(cancellationToken);
        var rows = await ExecuteAsync(
            session,
            BuildCategoriaQuery(
                "SELECT id_campeonato_categoria, id_campeonato, campeonato, id_categoria, categoria, id_equipo, equipo, activo, cantidad_jugadores_activos, fecha_creacion FROM reportes_equipos_by_categoria",
                campeonatoId,
                campeonatoCategoriaId),
            cancellationToken);

        return rows
            .Select(row => new EquipoRow(
                row.GetValue<int>("id_campeonato_categoria"),
                row.GetValue<int>("id_campeonato"),
                row.GetValue<string>("campeonato"),
                row.GetValue<int>("id_categoria"),
                row.GetValue<string>("categoria"),
                row.GetValue<int>("id_equipo"),
                row.GetValue<string>("equipo"),
                row.GetValue<bool>("activo"),
                row.GetValue<int>("cantidad_jugadores_activos"),
                row.GetValue<DateTimeOffset>("fecha_creacion").UtcDateTime))
            .GroupBy(row => row.IdCampeonatoCategoria)
            .Select(group =>
            {
                var first = group.First();
                var equipos = group
                    .OrderBy(row => row.Equipo)
                    .ThenBy(row => row.IdEquipo)
                    .Select(row => new ReporteEquipoResponse(
                        row.IdEquipo,
                        row.Equipo,
                        row.Activo,
                        row.CantidadJugadoresActivos,
                        row.FechaCreacion))
                    .ToList();

                return new ReporteEquiposCategoriaResponse(
                    first.IdCampeonatoCategoria,
                    first.IdCampeonato,
                    first.Campeonato,
                    first.IdCategoria,
                    first.Categoria,
                    equipos.Count,
                    equipos.Sum(equipo => equipo.CantidadJugadoresActivos),
                    equipos);
            })
            .OrderBy(reporte => reporte.Campeonato)
            .ThenBy(reporte => reporte.Categoria)
            .ToList();
    }

    public async Task<IReadOnlyCollection<ReporteJugadoresCategoriaResponse>> GetJugadoresAsync(
        int? campeonatoId,
        int? campeonatoCategoriaId,
        int? equipoId,
        CancellationToken cancellationToken)
    {
        var session = await _sessionFactory.CreateSessionAsync(cancellationToken);
        var query = BuildCategoriaQuery(
            "SELECT id_campeonato_categoria, id_campeonato, campeonato, id_categoria, categoria, id_equipo, equipo, id_jugador, id_persona, cedula, nombre, apellido, telefono, fecha_nacimiento, estado_inscripcion, fecha_inscripcion FROM reportes_jugadores_by_categoria",
            campeonatoId,
            campeonatoCategoriaId);

        if (equipoId.HasValue)
        {
            query = AddCondition(query, $"id_equipo = {equipoId.Value}");
        }

        var rows = await ExecuteAsync(session, query, cancellationToken);

        return rows
            .Select(CreateJugadorRow)
            .GroupBy(row => row.IdCampeonatoCategoria)
            .Select(group =>
            {
                var first = group.First();
                var equipos = group
                    .GroupBy(row => row.IdEquipo)
                    .Select(equipoGroup =>
                    {
                        var equipoFirst = equipoGroup.First();
                        var jugadores = equipoGroup
                            .OrderBy(row => row.Apellido)
                            .ThenBy(row => row.Nombre)
                            .ThenBy(row => row.IdJugador)
                            .Select(row => new ReporteJugadorResponse(
                                row.IdJugador,
                                row.IdPersona,
                                row.Cedula,
                                row.Nombre,
                                row.Apellido,
                                row.Telefono,
                                row.FechaNacimiento,
                                row.EstadoInscripcion,
                                row.FechaInscripcion))
                            .ToList();

                        return new ReporteJugadoresEquipoResponse(
                            equipoFirst.IdEquipo,
                            equipoFirst.Equipo,
                            jugadores.Count,
                            jugadores);
                    })
                    .OrderBy(equipo => equipo.Equipo)
                    .ToList();

                return new ReporteJugadoresCategoriaResponse(
                    first.IdCampeonatoCategoria,
                    first.IdCampeonato,
                    first.Campeonato,
                    first.IdCategoria,
                    first.Categoria,
                    equipos.Sum(equipo => equipo.TotalJugadores),
                    equipos);
            })
            .OrderBy(reporte => reporte.Campeonato)
            .ThenBy(reporte => reporte.Categoria)
            .ToList();
    }

    public async Task<IReadOnlyCollection<ReporteResultadoResponse>> GetResultadosAsync(
        int? campeonatoCategoriaId,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        CancellationToken cancellationToken)
    {
        var session = await _sessionFactory.CreateSessionAsync(cancellationToken);
        var query = "SELECT id_resultado, id_partido, id_campeonato_categoria, id_campeonato, campeonato, id_categoria, categoria, id_fase, fase, id_jornada, numero_jornada, id_equipo_local, equipo_local, id_equipo_visitante, equipo_visitante, sets_local, sets_visitante, id_equipo_ganador, equipo_ganador, fecha_registro, fecha_actualizacion, sets_json FROM reportes_resultados_by_categoria_fecha";

        if (campeonatoCategoriaId.HasValue)
        {
            query = AddCondition(query, $"id_campeonato_categoria = {campeonatoCategoriaId.Value}");
        }

        if (fechaDesde.HasValue)
        {
            query = AddCondition(query, $"fecha_registro >= '{fechaDesde.Value:yyyy-MM-dd}'");
        }

        if (fechaHasta.HasValue)
        {
            query = AddCondition(query, $"fecha_registro <= '{fechaHasta.Value:yyyy-MM-dd}'");
        }

        var rows = await ExecuteAsync(session, EnsureAllowFiltering(query), cancellationToken);

        return rows
            .Select(CreateResultadoResponse)
            .OrderByDescending(resultado => resultado.FechaRegistro)
            .ThenByDescending(resultado => resultado.IdResultado)
            .ToList();
    }

    public async Task<IReadOnlyCollection<ReportePosicionesCategoriaResponse>> GetPosicionesAsync(
        int? campeonatoId,
        int? campeonatoCategoriaId,
        CancellationToken cancellationToken)
    {
        var session = await _sessionFactory.CreateSessionAsync(cancellationToken);
        var rows = await ExecuteAsync(
            session,
            BuildCategoriaQuery(
                "SELECT id_campeonato_categoria, id_campeonato, campeonato, id_categoria, categoria, posicion, id_equipo, equipo, partidos_jugados, ganados, perdidos, sets_favor, sets_contra, puntos_favor, puntos_contra, puntos, fecha_actualizacion FROM reportes_posiciones_by_categoria",
                campeonatoId,
                campeonatoCategoriaId),
            cancellationToken);

        return rows
            .Select(CreatePosicionRow)
            .GroupBy(row => row.IdCampeonatoCategoria)
            .Select(group =>
            {
                var first = group.First();
                var posiciones = group
                    .OrderBy(row => row.Posicion)
                    .Select(row => new PosicionResponse(
                        row.Posicion,
                        row.IdEquipo,
                        row.Equipo,
                        row.PartidosJugados,
                        row.Ganados,
                        row.Perdidos,
                        row.SetsFavor,
                        row.SetsContra,
                        row.SetsFavor - row.SetsContra,
                        row.PuntosFavor,
                        row.PuntosContra,
                        row.PuntosFavor - row.PuntosContra,
                        row.Puntos,
                        row.FechaActualizacion))
                    .ToList();

                return new ReportePosicionesCategoriaResponse(
                    first.IdCampeonatoCategoria,
                    first.IdCampeonato,
                    first.Campeonato,
                    first.IdCategoria,
                    first.Categoria,
                    posiciones.Count == 0 ? null : posiciones.Max(posicion => posicion.FechaActualizacion),
                    posiciones);
            })
            .OrderBy(reporte => reporte.Campeonato)
            .ThenBy(reporte => reporte.Categoria)
            .ToList();
    }

    private static async Task<IReadOnlyCollection<Row>> ExecuteAsync(
        global::Cassandra.ISession session,
        string cql,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await session.ExecuteAsync(new SimpleStatement(cql));
        cancellationToken.ThrowIfCancellationRequested();

        return result.ToList();
    }

    private static string BuildCategoriaQuery(string baseQuery, int? campeonatoId, int? campeonatoCategoriaId)
    {
        var query = baseQuery;

        if (campeonatoId.HasValue)
        {
            query = AddCondition(query, $"id_campeonato = {campeonatoId.Value}");
        }

        if (campeonatoCategoriaId.HasValue)
        {
            query = AddCondition(query, $"id_campeonato_categoria = {campeonatoCategoriaId.Value}");
        }

        return EnsureAllowFiltering(query);
    }

    private static string AddCondition(string query, string condition)
    {
        return query.Contains(" WHERE ", StringComparison.OrdinalIgnoreCase)
            ? $"{query} AND {condition}"
            : $"{query} WHERE {condition}";
    }

    private static string EnsureAllowFiltering(string query)
    {
        return query.Contains(" WHERE ", StringComparison.OrdinalIgnoreCase)
            ? $"{query} ALLOW FILTERING"
            : query;
    }

    private static JugadorRow CreateJugadorRow(Row row)
    {
        return new JugadorRow(
            row.GetValue<int>("id_campeonato_categoria"),
            row.GetValue<int>("id_campeonato"),
            row.GetValue<string>("campeonato"),
            row.GetValue<int>("id_categoria"),
            row.GetValue<string>("categoria"),
            row.GetValue<int>("id_equipo"),
            row.GetValue<string>("equipo"),
            row.GetValue<int>("id_jugador"),
            row.GetValue<int?>("id_persona"),
            row.GetValue<string>("cedula"),
            row.GetValue<string>("nombre"),
            row.GetValue<string>("apellido"),
            row.GetValue<string?>("telefono"),
            row.GetValue<LocalDate?>("fecha_nacimiento") is { } fechaNacimiento
                ? new DateOnly(fechaNacimiento.Year, fechaNacimiento.Month, fechaNacimiento.Day)
                : null,
            row.GetValue<string>("estado_inscripcion"),
            row.GetValue<DateTimeOffset>("fecha_inscripcion").UtcDateTime);
    }

    private static ReporteResultadoResponse CreateResultadoResponse(Row row)
    {
        var equipoLocal = row.GetValue<string>("equipo_local");
        var equipoVisitante = row.GetValue<string>("equipo_visitante");
        var setsJson = row.GetValue<string?>("sets_json");
        var projectedSets = string.IsNullOrWhiteSpace(setsJson)
            ? []
            : JsonSerializer.Deserialize<IReadOnlyCollection<ResultadoSetProjection>>(setsJson) ?? [];

        var sets = projectedSets
            .Select(set =>
            {
                var puntosLocal = set.PuntosLocal;
                var puntosVisitante = set.PuntosVisitante;
                var localWon = puntosLocal > puntosVisitante;

                return new ResultadoSetResponse(
                    set.NumeroSet,
                    puntosLocal,
                    puntosVisitante,
                    localWon ? row.GetValue<int>("id_equipo_local") : row.GetValue<int>("id_equipo_visitante"),
                    localWon ? equipoLocal : equipoVisitante);
            })
            .OrderBy(set => set.NumeroSet)
            .ToList();

        return new ReporteResultadoResponse(
            row.GetValue<int>("id_resultado"),
            row.GetValue<int>("id_partido"),
            row.GetValue<int>("id_campeonato_categoria"),
            row.GetValue<int>("id_campeonato"),
            row.GetValue<string>("campeonato"),
            row.GetValue<int>("id_categoria"),
            row.GetValue<string>("categoria"),
            row.GetValue<int>("id_fase"),
            row.GetValue<string>("fase"),
            row.GetValue<int>("id_jornada"),
            row.GetValue<int>("numero_jornada"),
            row.GetValue<int>("id_equipo_local"),
            equipoLocal,
            row.GetValue<int>("id_equipo_visitante"),
            equipoVisitante,
            row.GetValue<int>("sets_local"),
            row.GetValue<int>("sets_visitante"),
            row.GetValue<int>("id_equipo_ganador"),
            row.GetValue<string>("equipo_ganador"),
            row.GetValue<DateTimeOffset>("fecha_registro").UtcDateTime,
            row.GetValue<DateTimeOffset?>("fecha_actualizacion")?.UtcDateTime,
            sets);
    }

    private static PosicionRow CreatePosicionRow(Row row)
    {
        return new PosicionRow(
            row.GetValue<int>("id_campeonato_categoria"),
            row.GetValue<int>("id_campeonato"),
            row.GetValue<string>("campeonato"),
            row.GetValue<int>("id_categoria"),
            row.GetValue<string>("categoria"),
            row.GetValue<int>("posicion"),
            row.GetValue<int>("id_equipo"),
            row.GetValue<string>("equipo"),
            row.GetValue<int>("partidos_jugados"),
            row.GetValue<int>("ganados"),
            row.GetValue<int>("perdidos"),
            row.GetValue<int>("sets_favor"),
            row.GetValue<int>("sets_contra"),
            row.GetValue<int>("puntos_favor"),
            row.GetValue<int>("puntos_contra"),
            row.GetValue<int>("puntos"),
            row.GetValue<DateTimeOffset>("fecha_actualizacion").UtcDateTime);
    }

    private sealed record EquipoRow(
        int IdCampeonatoCategoria,
        int IdCampeonato,
        string Campeonato,
        int IdCategoria,
        string Categoria,
        int IdEquipo,
        string Equipo,
        bool Activo,
        int CantidadJugadoresActivos,
        DateTime FechaCreacion);

    private sealed record JugadorRow(
        int IdCampeonatoCategoria,
        int IdCampeonato,
        string Campeonato,
        int IdCategoria,
        string Categoria,
        int IdEquipo,
        string Equipo,
        int IdJugador,
        int? IdPersona,
        string Cedula,
        string Nombre,
        string Apellido,
        string? Telefono,
        DateOnly? FechaNacimiento,
        string EstadoInscripcion,
        DateTime FechaInscripcion);

    private sealed record PosicionRow(
        int IdCampeonatoCategoria,
        int IdCampeonato,
        string Campeonato,
        int IdCategoria,
        string Categoria,
        int Posicion,
        int IdEquipo,
        string Equipo,
        int PartidosJugados,
        int Ganados,
        int Perdidos,
        int SetsFavor,
        int SetsContra,
        int PuntosFavor,
        int PuntosContra,
        int Puntos,
        DateTime FechaActualizacion);

    private sealed record ResultadoSetProjection(
        int NumeroSet,
        int PuntosLocal,
        int PuntosVisitante);
}
