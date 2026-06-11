using Microsoft.EntityFrameworkCore;
using WallyBallBackend.Application.Posiciones;
using WallyBallBackend.Application.Reportes;
using WallyBallBackend.Application.Resultados;
using WallyBallBackend.Domain.Entities;
using WallyBallBackend.Infrastructure.Persistence.SqlServer;
using WallyBallBackend.Infrastructure.Personas;

namespace WallyBallBackend.Infrastructure.Reportes;

public sealed class ReporteService : IReporteService
{
    private const string EstadoInscripcionActiva = "ACTIVO";

    private readonly AppDbContext _dbContext;
    private readonly IPersonasServiceClient _personasServiceClient;

    public ReporteService(AppDbContext dbContext, IPersonasServiceClient personasServiceClient)
    {
        _dbContext = dbContext;
        _personasServiceClient = personasServiceClient;
    }

    public async Task<IReadOnlyCollection<ReporteEquiposCategoriaResponse>> GetReporteEquiposAsync(
        int? campeonatoId,
        int? campeonatoCategoriaId,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.CampeonatoCategorias
            .AsNoTracking()
            .Include(campeonatoCategoria => campeonatoCategoria.Campeonato)
            .Include(campeonatoCategoria => campeonatoCategoria.Categoria)
            .Include(campeonatoCategoria => campeonatoCategoria.Equipos)
            .ThenInclude(equipo => equipo.Inscripciones)
            .AsQueryable();

        query = ApplyCategoriaFilters(query, campeonatoId, campeonatoCategoriaId);

        var categorias = await query
            .OrderBy(campeonatoCategoria => campeonatoCategoria.Campeonato!.Nombre)
            .ThenBy(campeonatoCategoria => campeonatoCategoria.Categoria!.Nombre)
            .ThenBy(campeonatoCategoria => campeonatoCategoria.IdCampeonatoCategoria)
            .ToListAsync(cancellationToken);

        return categorias.Select(CreateReporteEquiposCategoriaResponse).ToList();
    }

    public async Task<IReadOnlyCollection<ReporteJugadoresCategoriaResponse>> GetReporteJugadoresAsync(
        int? campeonatoId,
        int? campeonatoCategoriaId,
        int? equipoId,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.CampeonatoCategorias
            .AsNoTracking()
            .Include(campeonatoCategoria => campeonatoCategoria.Campeonato)
            .Include(campeonatoCategoria => campeonatoCategoria.Categoria)
            .Include(campeonatoCategoria => campeonatoCategoria.Equipos)
            .ThenInclude(equipo => equipo.Inscripciones)
            .ThenInclude(inscripcion => inscripcion.Jugador)
            .AsQueryable();

        query = ApplyCategoriaFilters(query, campeonatoId, campeonatoCategoriaId);

        if (equipoId.HasValue)
        {
            query = query.Where(campeonatoCategoria => campeonatoCategoria.Equipos.Any(equipo => equipo.IdEquipo == equipoId.Value));
        }

        var categorias = await query
            .OrderBy(campeonatoCategoria => campeonatoCategoria.Campeonato!.Nombre)
            .ThenBy(campeonatoCategoria => campeonatoCategoria.Categoria!.Nombre)
            .ThenBy(campeonatoCategoria => campeonatoCategoria.IdCampeonatoCategoria)
            .ToListAsync(cancellationToken);

        var personaIds = categorias
            .SelectMany(categoria => categoria.Equipos)
            .Where(equipo => !equipoId.HasValue || equipo.IdEquipo == equipoId.Value)
            .SelectMany(equipo => equipo.Inscripciones)
            .Where(inscripcion => inscripcion.Jugador?.IdPersona is not null)
            .Select(inscripcion => inscripcion.Jugador!.IdPersona!.Value)
            .Distinct()
            .ToArray();

        var personas = await GetPersonasByIdsAsync(personaIds, cancellationToken);

        return categorias
            .Select(categoria => CreateReporteJugadoresCategoriaResponse(categoria, equipoId, personas))
            .ToList();
    }

    public async Task<IReadOnlyCollection<ReporteResultadoResponse>> GetReporteResultadosAsync(
        int? campeonatoCategoriaId,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Resultados
            .AsNoTracking()
            .Include(resultado => resultado.Partido)
            .ThenInclude(partido => partido!.CampeonatoCategoria)
            .ThenInclude(campeonatoCategoria => campeonatoCategoria!.Campeonato)
            .Include(resultado => resultado.Partido)
            .ThenInclude(partido => partido!.CampeonatoCategoria)
            .ThenInclude(campeonatoCategoria => campeonatoCategoria!.Categoria)
            .Include(resultado => resultado.Partido)
            .ThenInclude(partido => partido!.Fase)
            .Include(resultado => resultado.Partido)
            .ThenInclude(partido => partido!.Jornada)
            .Include(resultado => resultado.Partido)
            .ThenInclude(partido => partido!.EquipoLocal)
            .Include(resultado => resultado.Partido)
            .ThenInclude(partido => partido!.EquipoVisitante)
            .Include(resultado => resultado.EquipoGanador)
            .Include(resultado => resultado.Sets)
            .AsQueryable();

        if (campeonatoCategoriaId.HasValue)
        {
            query = query.Where(resultado => resultado.Partido!.IdCampeonatoCategoria == campeonatoCategoriaId.Value);
        }

        if (fechaDesde.HasValue)
        {
            var desde = fechaDesde.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(resultado => resultado.FechaRegistro >= desde);
        }

        if (fechaHasta.HasValue)
        {
            var hasta = fechaHasta.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(resultado => resultado.FechaRegistro <= hasta);
        }

        var resultados = await query
            .OrderBy(resultado => resultado.Partido!.CampeonatoCategoria!.Campeonato!.Nombre)
            .ThenBy(resultado => resultado.Partido!.CampeonatoCategoria!.Categoria!.Nombre)
            .ThenByDescending(resultado => resultado.FechaRegistro)
            .ThenByDescending(resultado => resultado.IdResultado)
            .ToListAsync(cancellationToken);

        return resultados.Select(CreateReporteResultadoResponse).ToList();
    }

    public async Task<IReadOnlyCollection<ReportePosicionesCategoriaResponse>> GetReportePosicionesAsync(
        int? campeonatoId,
        int? campeonatoCategoriaId,
        CancellationToken cancellationToken)
    {
        var query = ApplyCategoriaFilters(
            _dbContext.CampeonatoCategorias
                .AsNoTracking()
                .Include(campeonatoCategoria => campeonatoCategoria.Campeonato)
                .Include(campeonatoCategoria => campeonatoCategoria.Categoria)
                .AsQueryable(),
            campeonatoId,
            campeonatoCategoriaId);

        var categorias = await query
            .OrderBy(campeonatoCategoria => campeonatoCategoria.Campeonato!.Nombre)
            .ThenBy(campeonatoCategoria => campeonatoCategoria.Categoria!.Nombre)
            .ThenBy(campeonatoCategoria => campeonatoCategoria.IdCampeonatoCategoria)
            .ToListAsync(cancellationToken);

        var response = new List<ReportePosicionesCategoriaResponse>();

        foreach (var categoria in categorias)
        {
            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC SP_CrearTablaPosicionesCategoria {categoria.IdCampeonatoCategoria}",
                cancellationToken);

            var posiciones = await _dbContext.TablaPosiciones
                .AsNoTracking()
                .Include(posicion => posicion.Equipo)
                .Where(posicion => posicion.IdCampeonatoCategoria == categoria.IdCampeonatoCategoria)
                .OrderByDescending(posicion => posicion.Puntos)
                .ThenByDescending(posicion => posicion.Ganados)
                .ThenByDescending(posicion => posicion.SetsFavor - posicion.SetsContra)
                .ThenByDescending(posicion => posicion.PuntosFavor - posicion.PuntosContra)
                .ThenByDescending(posicion => posicion.SetsFavor)
                .ThenByDescending(posicion => posicion.PuntosFavor)
                .ThenBy(posicion => posicion.Equipo!.Nombre)
                .ToListAsync(cancellationToken);

            var tabla = posiciones
                .Select((posicion, index) => CreatePosicionResponse(posicion, index + 1))
                .ToList();

            response.Add(new ReportePosicionesCategoriaResponse(
                categoria.IdCampeonatoCategoria,
                categoria.IdCampeonato,
                categoria.Campeonato?.Nombre ?? string.Empty,
                categoria.IdCategoria,
                categoria.Categoria?.Nombre ?? string.Empty,
                tabla.Count == 0 ? null : tabla.Max(posicion => posicion.FechaActualizacion),
                tabla));
        }

        return response;
    }

    private static IQueryable<CampeonatoCategoria> ApplyCategoriaFilters(
        IQueryable<CampeonatoCategoria> query,
        int? campeonatoId,
        int? campeonatoCategoriaId)
    {
        if (campeonatoId.HasValue)
        {
            query = query.Where(campeonatoCategoria => campeonatoCategoria.IdCampeonato == campeonatoId.Value);
        }

        if (campeonatoCategoriaId.HasValue)
        {
            query = query.Where(campeonatoCategoria => campeonatoCategoria.IdCampeonatoCategoria == campeonatoCategoriaId.Value);
        }

        return query;
    }

    private async Task<Dictionary<int, PersonaClientResponse>> GetPersonasByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        var result = await _personasServiceClient.GetPersonasAsync(ids, null, null, cancellationToken);

        return result.Succeeded
            ? result.Value!.ToDictionary(persona => persona.IdPersona)
            : [];
    }

    private static ReporteEquiposCategoriaResponse CreateReporteEquiposCategoriaResponse(CampeonatoCategoria categoria)
    {
        var equipos = categoria.Equipos
            .OrderBy(equipo => equipo.Nombre)
            .ThenBy(equipo => equipo.IdEquipo)
            .Select(equipo => new ReporteEquipoResponse(
                equipo.IdEquipo,
                equipo.Nombre,
                equipo.Activo,
                equipo.Inscripciones.Count(inscripcion => inscripcion.Estado == EstadoInscripcionActiva),
                equipo.FechaCreacion))
            .ToList();

        return new ReporteEquiposCategoriaResponse(
            categoria.IdCampeonatoCategoria,
            categoria.IdCampeonato,
            categoria.Campeonato?.Nombre ?? string.Empty,
            categoria.IdCategoria,
            categoria.Categoria?.Nombre ?? string.Empty,
            equipos.Count,
            equipos.Sum(equipo => equipo.CantidadJugadoresActivos),
            equipos);
    }

    private static ReporteJugadoresCategoriaResponse CreateReporteJugadoresCategoriaResponse(
        CampeonatoCategoria categoria,
        int? equipoId,
        IReadOnlyDictionary<int, PersonaClientResponse> personas)
    {
        var equipos = categoria.Equipos
            .Where(equipo => !equipoId.HasValue || equipo.IdEquipo == equipoId.Value)
            .OrderBy(equipo => equipo.Nombre)
            .ThenBy(equipo => equipo.IdEquipo)
            .Select(equipo => CreateReporteJugadoresEquipoResponse(equipo, personas))
            .ToList();

        return new ReporteJugadoresCategoriaResponse(
            categoria.IdCampeonatoCategoria,
            categoria.IdCampeonato,
            categoria.Campeonato?.Nombre ?? string.Empty,
            categoria.IdCategoria,
            categoria.Categoria?.Nombre ?? string.Empty,
            equipos.Sum(equipo => equipo.TotalJugadores),
            equipos);
    }

    private static ReporteJugadoresEquipoResponse CreateReporteJugadoresEquipoResponse(
        Equipo equipo,
        IReadOnlyDictionary<int, PersonaClientResponse> personas)
    {
        var jugadores = equipo.Inscripciones
            .OrderBy(inscripcion => inscripcion.Jugador?.IdJugador)
            .Select(inscripcion => CreateReporteJugadorResponse(inscripcion, personas))
            .OrderBy(jugador => jugador.Apellido)
            .ThenBy(jugador => jugador.Nombre)
            .ThenBy(jugador => jugador.IdJugador)
            .ToList();

        return new ReporteJugadoresEquipoResponse(
            equipo.IdEquipo,
            equipo.Nombre,
            jugadores.Count,
            jugadores);
    }

    private static ReporteJugadorResponse CreateReporteJugadorResponse(
        InscripcionEquipoJugador inscripcion,
        IReadOnlyDictionary<int, PersonaClientResponse> personas)
    {
        var jugador = inscripcion.Jugador;
        var persona = jugador?.IdPersona is null ? null : personas.GetValueOrDefault(jugador.IdPersona.Value);

        return new ReporteJugadorResponse(
            inscripcion.IdJugador,
            jugador?.IdPersona,
            persona?.Cedula ?? string.Empty,
            persona?.Nombre ?? string.Empty,
            persona?.Apellido ?? string.Empty,
            persona?.Telefono,
            persona?.FechaNacimiento,
            inscripcion.Estado,
            inscripcion.FechaInscripcion);
    }

    private static ReporteResultadoResponse CreateReporteResultadoResponse(Resultado resultado)
    {
        var partido = resultado.Partido!;
        var campeonatoCategoria = partido.CampeonatoCategoria!;
        var equipoLocal = partido.EquipoLocal!;
        var equipoVisitante = partido.EquipoVisitante!;

        return new ReporteResultadoResponse(
            resultado.IdResultado,
            resultado.IdPartido,
            partido.IdCampeonatoCategoria,
            campeonatoCategoria.IdCampeonato,
            campeonatoCategoria.Campeonato?.Nombre ?? string.Empty,
            campeonatoCategoria.IdCategoria,
            campeonatoCategoria.Categoria?.Nombre ?? string.Empty,
            partido.IdFase,
            partido.Fase?.Nombre ?? string.Empty,
            partido.IdJornada,
            partido.Jornada?.NumeroJornada ?? 0,
            partido.IdEquipoLocal,
            equipoLocal.Nombre,
            partido.IdEquipoVisitante,
            equipoVisitante.Nombre,
            resultado.SetsLocal,
            resultado.SetsVisitante,
            resultado.IdEquipoGanador,
            resultado.EquipoGanador?.Nombre ?? string.Empty,
            resultado.FechaRegistro,
            resultado.FechaActualizacion,
            resultado.Sets
                .OrderBy(set => set.NumeroSet)
                .Select(set => new ResultadoSetResponse(
                    set.IdResultadoSet,
                    set.NumeroSet,
                    set.PuntosLocal,
                    set.PuntosVisitante,
                    set.PuntosLocal > set.PuntosVisitante ? partido.IdEquipoLocal : partido.IdEquipoVisitante,
                    set.PuntosLocal > set.PuntosVisitante ? equipoLocal.Nombre : equipoVisitante.Nombre))
                .ToList());
    }

    private static PosicionResponse CreatePosicionResponse(TablaPosicion posicion, int index)
    {
        return new PosicionResponse(
            index,
            posicion.IdPosicion,
            posicion.IdCampeonatoCategoria,
            posicion.IdEquipo,
            posicion.Equipo?.Nombre ?? string.Empty,
            posicion.PartidosJugados,
            posicion.Ganados,
            posicion.Perdidos,
            posicion.SetsFavor,
            posicion.SetsContra,
            posicion.SetsFavor - posicion.SetsContra,
            posicion.PuntosFavor,
            posicion.PuntosContra,
            posicion.PuntosFavor - posicion.PuntosContra,
            posicion.Puntos,
            posicion.FechaActualizacion);
    }
}
