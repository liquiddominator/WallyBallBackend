using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WallyBallBackend.Application.PortalJugador;
using WallyBallBackend.Application.Posiciones;
using WallyBallBackend.Application.Resultados;
using WallyBallBackend.Domain.Entities;
using WallyBallBackend.Infrastructure.Persistence.SqlServer;

namespace WallyBallBackend.Infrastructure.PortalJugador;

public sealed class PortalJugadorService : IPortalJugadorService
{
    private const string EstadoInscripcionActiva = "ACTIVO";
    private const string EstadoPartidoFinalizado = "FINALIZADO";
    private const string EstadoPartidoCancelado = "CANCELADO";

    private readonly AppDbContext _dbContext;

    public PortalJugadorService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PortalJugadorOperationResult<IReadOnlyCollection<PortalFixturePartidoResponse>>> GetFixturePersonalAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var context = await GetJugadorContextAsync(user, cancellationToken);

        if (!context.Succeeded)
        {
            return PortalJugadorOperationResult<IReadOnlyCollection<PortalFixturePartidoResponse>>.Failure(
                context.ErrorCode!,
                context.ErrorMessage!);
        }

        var equipoIds = context.Value!.Equipos.Select(equipo => equipo.IdEquipo).ToArray();
        var now = DateTime.UtcNow;

        var partidos = await CreatePartidoQuery()
            .Where(partido =>
                (equipoIds.Contains(partido.IdEquipoLocal) || equipoIds.Contains(partido.IdEquipoVisitante))
                && partido.Estado != EstadoPartidoFinalizado
                && partido.Estado != EstadoPartidoCancelado
                && (!partido.FechaHoraProgramada.HasValue || partido.FechaHoraProgramada.Value >= now))
            .OrderBy(partido => partido.FechaHoraProgramada == null)
            .ThenBy(partido => partido.FechaHoraProgramada)
            .ThenBy(partido => partido.Jornada!.NumeroJornada)
            .ThenBy(partido => partido.IdPartido)
            .ToListAsync(cancellationToken);

        var equiposById = context.Value.Equipos.ToDictionary(equipo => equipo.IdEquipo);

        return PortalJugadorOperationResult<IReadOnlyCollection<PortalFixturePartidoResponse>>.Success(
            partidos.Select(partido => CreateFixturePartidoResponse(partido, equiposById)).ToList());
    }

    public async Task<PortalJugadorOperationResult<IReadOnlyCollection<PortalResultadosCategoriaResponse>>> GetResultadosAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var context = await GetJugadorContextAsync(user, cancellationToken);

        if (!context.Succeeded)
        {
            return PortalJugadorOperationResult<IReadOnlyCollection<PortalResultadosCategoriaResponse>>.Failure(
                context.ErrorCode!,
                context.ErrorMessage!);
        }

        var campeonatoCategoriaIds = context.Value!.Equipos
            .Select(equipo => equipo.IdCampeonatoCategoria)
            .Distinct()
            .ToArray();

        var resultados = await _dbContext.Resultados
            .AsNoTracking()
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
            .Where(resultado => campeonatoCategoriaIds.Contains(resultado.Partido!.IdCampeonatoCategoria))
            .OrderByDescending(resultado => resultado.FechaRegistro)
            .ThenByDescending(resultado => resultado.IdResultado)
            .ToListAsync(cancellationToken);

        var resultadosByCategoria = resultados
            .GroupBy(resultado => resultado.Partido!.IdCampeonatoCategoria)
            .ToDictionary(group => group.Key, group => group.Select(CreateResultadoResponse).ToList());

        var response = context.Value.Equipos
            .OrderBy(equipo => equipo.CampeonatoCategoria?.Campeonato?.Nombre)
            .ThenBy(equipo => equipo.CampeonatoCategoria?.Categoria?.Nombre)
            .ThenBy(equipo => equipo.Nombre)
            .Select(equipo => CreateResultadosCategoriaResponse(equipo, resultadosByCategoria))
            .ToList();

        return PortalJugadorOperationResult<IReadOnlyCollection<PortalResultadosCategoriaResponse>>.Success(response);
    }

    public async Task<PortalJugadorOperationResult<IReadOnlyCollection<PortalPosicionesCategoriaResponse>>> GetPosicionesAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var context = await GetJugadorContextAsync(user, cancellationToken);

        if (!context.Succeeded)
        {
            return PortalJugadorOperationResult<IReadOnlyCollection<PortalPosicionesCategoriaResponse>>.Failure(
                context.ErrorCode!,
                context.ErrorMessage!);
        }

        var response = new List<PortalPosicionesCategoriaResponse>();

        foreach (var equipo in context.Value!.Equipos
                     .OrderBy(equipo => equipo.CampeonatoCategoria?.Campeonato?.Nombre)
                     .ThenBy(equipo => equipo.CampeonatoCategoria?.Categoria?.Nombre)
                     .ThenBy(equipo => equipo.Nombre))
        {
            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC SP_CrearTablaPosicionesCategoria {equipo.IdCampeonatoCategoria}",
                cancellationToken);

            var posiciones = await _dbContext.TablaPosiciones
                .AsNoTracking()
                .Include(posicion => posicion.Equipo)
                .Where(posicion => posicion.IdCampeonatoCategoria == equipo.IdCampeonatoCategoria)
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

            response.Add(CreatePosicionesCategoriaResponse(equipo, tabla));
        }

        return PortalJugadorOperationResult<IReadOnlyCollection<PortalPosicionesCategoriaResponse>>.Success(response);
    }

    private async Task<PortalJugadorOperationResult<JugadorContext>> GetJugadorContextAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var personaIdClaim = user.FindFirst("persona_id")?.Value;

        if (!int.TryParse(personaIdClaim, out var personaId))
        {
            return PortalJugadorOperationResult<JugadorContext>.Failure(
                "persona_claim_missing",
                "El token del jugador no contiene persona_id.");
        }

        var jugador = await _dbContext.Jugadores
            .AsNoTracking()
            .Include(jugador => jugador.Inscripciones.Where(inscripcion => inscripcion.Estado == EstadoInscripcionActiva))
            .ThenInclude(inscripcion => inscripcion.Equipo)
            .ThenInclude(equipo => equipo!.CampeonatoCategoria)
            .ThenInclude(campeonatoCategoria => campeonatoCategoria!.Campeonato)
            .Include(jugador => jugador.Inscripciones.Where(inscripcion => inscripcion.Estado == EstadoInscripcionActiva))
            .ThenInclude(inscripcion => inscripcion.Equipo)
            .ThenInclude(equipo => equipo!.CampeonatoCategoria)
            .ThenInclude(campeonatoCategoria => campeonatoCategoria!.Categoria)
            .SingleOrDefaultAsync(jugador => jugador.IdPersona == personaId && jugador.Activo, cancellationToken);

        if (jugador is null)
        {
            return PortalJugadorOperationResult<JugadorContext>.Failure(
                "player_not_found",
                "No existe un jugador activo vinculado a la persona del token.");
        }

        var equipos = jugador.Inscripciones
            .Where(inscripcion => inscripcion.Equipo is not null && inscripcion.Equipo.Activo)
            .Select(inscripcion => inscripcion.Equipo!)
            .DistinctBy(equipo => equipo.IdEquipo)
            .ToList();

        return PortalJugadorOperationResult<JugadorContext>.Success(new JugadorContext(jugador.IdJugador, personaId, equipos));
    }

    private IQueryable<Partido> CreatePartidoQuery()
    {
        return _dbContext.Partidos
            .AsNoTracking()
            .Include(partido => partido.CampeonatoCategoria)
            .ThenInclude(campeonatoCategoria => campeonatoCategoria!.Campeonato)
            .Include(partido => partido.CampeonatoCategoria)
            .ThenInclude(campeonatoCategoria => campeonatoCategoria!.Categoria)
            .Include(partido => partido.Fase)
            .Include(partido => partido.Jornada)
            .Include(partido => partido.EquipoLocal)
            .Include(partido => partido.EquipoVisitante);
    }

    private static PortalFixturePartidoResponse CreateFixturePartidoResponse(
        Partido partido,
        IReadOnlyDictionary<int, Equipo> equiposById)
    {
        var equipoJugador = equiposById.TryGetValue(partido.IdEquipoLocal, out var local)
            ? local
            : equiposById[partido.IdEquipoVisitante];

        return new PortalFixturePartidoResponse(
            partido.IdPartido,
            partido.IdCampeonatoCategoria,
            partido.CampeonatoCategoria?.IdCampeonato ?? 0,
            partido.CampeonatoCategoria?.Campeonato?.Nombre ?? string.Empty,
            partido.CampeonatoCategoria?.IdCategoria ?? 0,
            partido.CampeonatoCategoria?.Categoria?.Nombre ?? string.Empty,
            partido.IdFase,
            partido.Fase?.Nombre ?? string.Empty,
            partido.IdJornada,
            partido.Jornada?.NumeroJornada ?? 0,
            partido.Jornada?.FechaProgramada,
            equipoJugador.IdEquipo,
            equipoJugador.Nombre,
            partido.IdEquipoLocal,
            partido.EquipoLocal?.Nombre ?? string.Empty,
            partido.IdEquipoVisitante,
            partido.EquipoVisitante?.Nombre ?? string.Empty,
            partido.FechaHoraProgramada,
            partido.Estado);
    }

    private static PortalResultadosCategoriaResponse CreateResultadosCategoriaResponse(
        Equipo equipo,
        IReadOnlyDictionary<int, List<ResultadoResponse>> resultadosByCategoria)
    {
        var campeonatoCategoria = equipo.CampeonatoCategoria;
        resultadosByCategoria.TryGetValue(equipo.IdCampeonatoCategoria, out var resultados);

        return new PortalResultadosCategoriaResponse(
            equipo.IdCampeonatoCategoria,
            campeonatoCategoria?.IdCampeonato ?? 0,
            campeonatoCategoria?.Campeonato?.Nombre ?? string.Empty,
            campeonatoCategoria?.IdCategoria ?? 0,
            campeonatoCategoria?.Categoria?.Nombre ?? string.Empty,
            equipo.IdEquipo,
            equipo.Nombre,
            resultados ?? []);
    }

    private static PortalPosicionesCategoriaResponse CreatePosicionesCategoriaResponse(
        Equipo equipo,
        IReadOnlyCollection<PosicionResponse> posiciones)
    {
        var campeonatoCategoria = equipo.CampeonatoCategoria;
        var posicionEquipo = posiciones.SingleOrDefault(posicion => posicion.IdEquipo == equipo.IdEquipo)?.Posicion;

        return new PortalPosicionesCategoriaResponse(
            equipo.IdCampeonatoCategoria,
            campeonatoCategoria?.IdCampeonato ?? 0,
            campeonatoCategoria?.Campeonato?.Nombre ?? string.Empty,
            campeonatoCategoria?.IdCategoria ?? 0,
            campeonatoCategoria?.Categoria?.Nombre ?? string.Empty,
            equipo.IdEquipo,
            equipo.Nombre,
            posicionEquipo,
            posiciones);
    }

    private static ResultadoResponse CreateResultadoResponse(Resultado resultado)
    {
        var partido = resultado.Partido!;
        var equipoLocal = partido.EquipoLocal!;
        var equipoVisitante = partido.EquipoVisitante!;

        return new ResultadoResponse(
            resultado.IdResultado,
            resultado.IdPartido,
            partido.IdCampeonatoCategoria,
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

    private sealed record JugadorContext(
        int IdJugador,
        int IdPersona,
        IReadOnlyCollection<Equipo> Equipos);
}
