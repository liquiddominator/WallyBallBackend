using Microsoft.EntityFrameworkCore;
using WallyBallBackend.Application.Jugadores;
using WallyBallBackend.Domain.Entities;
using WallyBallBackend.Infrastructure.Persistence.SqlServer;

namespace WallyBallBackend.Infrastructure.Jugadores;

public sealed class JugadorService : IJugadorService
{
    private const string EstadoCampeonatoActivo = "ACTIVO";
    private const string EstadoInscripcionActiva = "ACTIVO";
    private const int MaxJugadoresActivosPorEquipo = 12;

    private readonly AppDbContext _dbContext;

    public JugadorService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<JugadorResponse>> GetJugadoresAsync(
        string? termino,
        string? cedula,
        int? equipoId,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Jugadores
            .AsNoTracking()
            .AsQueryable();

        var normalizedTerm = NormalizeOptionalText(termino);
        if (normalizedTerm is not null)
        {
            query = query.Where(jugador =>
                jugador.Nombre.Contains(normalizedTerm)
                || jugador.Apellido.Contains(normalizedTerm)
                || (jugador.Nombre + " " + jugador.Apellido).Contains(normalizedTerm));
        }

        var normalizedCedula = NormalizeOptionalText(cedula);
        if (normalizedCedula is not null)
        {
            query = query.Where(jugador => jugador.Cedula.Contains(normalizedCedula));
        }

        if (equipoId.HasValue)
        {
            query = query.Where(jugador =>
                jugador.Inscripciones.Any(inscripcion =>
                    inscripcion.IdEquipo == equipoId.Value
                    && inscripcion.Estado == EstadoInscripcionActiva));
        }

        var jugadores = await query
            .Include(jugador => jugador.Inscripciones)
            .ThenInclude(inscripcion => inscripcion.Equipo)
            .ThenInclude(equipo => equipo!.CampeonatoCategoria)
            .ThenInclude(campeonatoCategoria => campeonatoCategoria!.Campeonato)
            .Include(jugador => jugador.Inscripciones)
            .ThenInclude(inscripcion => inscripcion.Equipo)
            .ThenInclude(equipo => equipo!.CampeonatoCategoria)
            .ThenInclude(campeonatoCategoria => campeonatoCategoria!.Categoria)
            .OrderBy(jugador => jugador.Apellido)
            .ThenBy(jugador => jugador.Nombre)
            .ThenBy(jugador => jugador.IdJugador)
            .ToListAsync(cancellationToken);

        return jugadores.Select(CreateJugadorResponse).ToList();
    }

    public async Task<JugadorResponse?> GetJugadorByIdAsync(int jugadorId, CancellationToken cancellationToken)
    {
        var jugador = await _dbContext.Jugadores
            .AsNoTracking()
            .Include(jugador => jugador.Inscripciones)
            .ThenInclude(inscripcion => inscripcion.Equipo)
            .ThenInclude(equipo => equipo!.CampeonatoCategoria)
            .ThenInclude(campeonatoCategoria => campeonatoCategoria!.Campeonato)
            .Include(jugador => jugador.Inscripciones)
            .ThenInclude(inscripcion => inscripcion.Equipo)
            .ThenInclude(equipo => equipo!.CampeonatoCategoria)
            .ThenInclude(campeonatoCategoria => campeonatoCategoria!.Categoria)
            .SingleOrDefaultAsync(jugador => jugador.IdJugador == jugadorId, cancellationToken);

        return jugador is null ? null : CreateJugadorResponse(jugador);
    }

    public async Task<IReadOnlyCollection<JugadorEquipoResponse>> GetJugadoresByEquipoAsync(
        int equipoId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.InscripcionesEquipoJugador
            .AsNoTracking()
            .Where(inscripcion => inscripcion.IdEquipo == equipoId && inscripcion.Estado == EstadoInscripcionActiva)
            .OrderBy(inscripcion => inscripcion.Jugador!.Apellido)
            .ThenBy(inscripcion => inscripcion.Jugador!.Nombre)
            .ThenBy(inscripcion => inscripcion.IdInscripcion)
            .Select(inscripcion => new JugadorEquipoResponse(
                inscripcion.IdInscripcion,
                inscripcion.IdJugador,
                inscripcion.IdEquipo,
                inscripcion.Equipo!.Nombre,
                inscripcion.Equipo.CampeonatoCategoria!.IdCampeonatoCategoria,
                inscripcion.Equipo.CampeonatoCategoria.Categoria!.Nombre,
                inscripcion.Equipo.CampeonatoCategoria.IdCampeonato,
                inscripcion.Equipo.CampeonatoCategoria.Campeonato!.Nombre,
                inscripcion.Jugador!.Cedula,
                inscripcion.Jugador.Nombre,
                inscripcion.Jugador.Apellido,
                inscripcion.Jugador.Telefono,
                inscripcion.Jugador.FechaNacimiento,
                inscripcion.Estado,
                inscripcion.FechaInscripcion))
            .ToListAsync(cancellationToken);
    }

    public async Task<JugadorOperationResult> CreateJugadorAsync(
        CreateJugadorRequest request,
        CancellationToken cancellationToken)
    {
        var cedula = request.Cedula.Trim();
        var nombre = request.Nombre.Trim();
        var apellido = request.Apellido.Trim();

        var exists = await _dbContext.Jugadores
            .AnyAsync(jugador => jugador.Cedula == cedula, cancellationToken);

        if (exists)
        {
            return JugadorOperationResult.Failure("duplicate_player", "Ya existe un jugador con esa cedula.");
        }

        var jugador = new Jugador
        {
            Cedula = cedula,
            Nombre = nombre,
            Apellido = apellido,
            Telefono = NormalizeOptionalText(request.Telefono),
            FechaNacimiento = request.FechaNacimiento,
            Activo = true
        };

        _dbContext.Jugadores.Add(jugador);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicatePlayerException(exception))
        {
            return JugadorOperationResult.Failure("duplicate_player", "Ya existe un jugador con esa cedula.");
        }
        catch (DbUpdateException)
        {
            return JugadorOperationResult.Failure("player_persistence_error", "No se pudo registrar el jugador en la base de datos.");
        }

        return JugadorOperationResult.Success(CreateJugadorResponse(jugador));
    }

    public async Task<JugadorOperationResult> AsignarJugadorEquipoAsync(
        int equipoId,
        AsignarJugadorEquipoRequest request,
        CancellationToken cancellationToken)
    {
        var equipo = await _dbContext.Equipos
            .Include(equipo => equipo.CampeonatoCategoria)
            .ThenInclude(campeonatoCategoria => campeonatoCategoria!.Campeonato)
            .Include(equipo => equipo.CampeonatoCategoria)
            .ThenInclude(campeonatoCategoria => campeonatoCategoria!.Categoria)
            .Include(equipo => equipo.Inscripciones)
            .SingleOrDefaultAsync(equipo => equipo.IdEquipo == equipoId, cancellationToken);

        if (equipo is null)
        {
            return JugadorOperationResult.Failure("team_not_found", "Equipo no encontrado.");
        }

        if (!equipo.Activo)
        {
            return JugadorOperationResult.Failure("inactive_team", "No se pueden asignar jugadores a un equipo inactivo.");
        }

        if (equipo.CampeonatoCategoria?.Campeonato is null
            || !string.Equals(equipo.CampeonatoCategoria.Campeonato.Estado, EstadoCampeonatoActivo, StringComparison.OrdinalIgnoreCase))
        {
            return JugadorOperationResult.Failure("inactive_championship", "No se pueden asignar jugadores en un campeonato finalizado o inactivo.");
        }

        var jugador = await _dbContext.Jugadores
            .SingleOrDefaultAsync(jugador => jugador.IdJugador == request.IdJugador, cancellationToken);

        if (jugador is null)
        {
            return JugadorOperationResult.Failure("player_not_found", "Jugador no encontrado.");
        }

        if (!jugador.Activo)
        {
            return JugadorOperationResult.Failure("inactive_player", "No se puede asignar un jugador inactivo.");
        }

        var alreadyAssigned = equipo.Inscripciones
            .Any(inscripcion => inscripcion.IdJugador == request.IdJugador);

        if (alreadyAssigned)
        {
            return JugadorOperationResult.Failure("duplicate_assignment", "El jugador ya esta asignado a este equipo.");
        }

        var activePlayers = equipo.Inscripciones
            .Count(inscripcion => string.Equals(inscripcion.Estado, EstadoInscripcionActiva, StringComparison.OrdinalIgnoreCase));

        if (activePlayers >= MaxJugadoresActivosPorEquipo)
        {
            return JugadorOperationResult.Failure("team_full", "El equipo no puede superar 12 jugadores activos.");
        }

        var inscripcion = new InscripcionEquipoJugador
        {
            IdEquipo = equipoId,
            Equipo = equipo,
            IdJugador = request.IdJugador,
            Jugador = jugador,
            Estado = EstadoInscripcionActiva,
            FechaInscripcion = DateTime.UtcNow
        };

        _dbContext.InscripcionesEquipoJugador.Add(inscripcion);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicateAssignmentException(exception))
        {
            return JugadorOperationResult.Failure("duplicate_assignment", "El jugador ya esta asignado a este equipo.");
        }
        catch (DbUpdateException exception) when (IsTeamFullException(exception))
        {
            return JugadorOperationResult.Failure("team_full", "El equipo no puede superar 12 jugadores activos.");
        }
        catch (DbUpdateException)
        {
            return JugadorOperationResult.Failure("assignment_persistence_error", "No se pudo asignar el jugador al equipo.");
        }

        return JugadorOperationResult.Success(CreateJugadorEquipoResponse(inscripcion));
    }

    private static JugadorResponse CreateJugadorResponse(Jugador jugador)
    {
        var equipos = jugador.Inscripciones
            .OrderBy(inscripcion => inscripcion.Equipo?.CampeonatoCategoria?.Campeonato?.Nombre)
            .ThenBy(inscripcion => inscripcion.Equipo?.CampeonatoCategoria?.Categoria?.Nombre)
            .ThenBy(inscripcion => inscripcion.Equipo?.Nombre)
            .Select(inscripcion => new JugadorEquipoResumenResponse(
                inscripcion.IdInscripcion,
                inscripcion.IdEquipo,
                inscripcion.Equipo?.Nombre ?? string.Empty,
                inscripcion.Equipo?.CampeonatoCategoria?.IdCampeonatoCategoria ?? 0,
                inscripcion.Equipo?.CampeonatoCategoria?.Categoria?.Nombre ?? string.Empty,
                inscripcion.Equipo?.CampeonatoCategoria?.IdCampeonato ?? 0,
                inscripcion.Equipo?.CampeonatoCategoria?.Campeonato?.Nombre ?? string.Empty,
                inscripcion.Estado,
                inscripcion.FechaInscripcion))
            .ToList();

        return new JugadorResponse(
            jugador.IdJugador,
            jugador.Cedula,
            jugador.Nombre,
            jugador.Apellido,
            jugador.Telefono,
            jugador.FechaNacimiento,
            jugador.Activo,
            jugador.FechaCreacion,
            jugador.FechaActualizacion,
            equipos);
    }

    private static JugadorEquipoResponse CreateJugadorEquipoResponse(InscripcionEquipoJugador inscripcion)
    {
        return new JugadorEquipoResponse(
            inscripcion.IdInscripcion,
            inscripcion.IdJugador,
            inscripcion.IdEquipo,
            inscripcion.Equipo?.Nombre ?? string.Empty,
            inscripcion.Equipo?.CampeonatoCategoria?.IdCampeonatoCategoria ?? 0,
            inscripcion.Equipo?.CampeonatoCategoria?.Categoria?.Nombre ?? string.Empty,
            inscripcion.Equipo?.CampeonatoCategoria?.IdCampeonato ?? 0,
            inscripcion.Equipo?.CampeonatoCategoria?.Campeonato?.Nombre ?? string.Empty,
            inscripcion.Jugador?.Cedula ?? string.Empty,
            inscripcion.Jugador?.Nombre ?? string.Empty,
            inscripcion.Jugador?.Apellido ?? string.Empty,
            inscripcion.Jugador?.Telefono,
            inscripcion.Jugador?.FechaNacimiento,
            inscripcion.Estado,
            inscripcion.FechaInscripcion);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool IsDuplicatePlayerException(DbUpdateException exception)
    {
        return ContainsDatabaseMessage(exception, "UQ_Jugadores_Cedula")
            || ContainsDatabaseMessage(exception, "Cannot insert duplicate key")
            || ContainsDatabaseMessage(exception, "Violation of UNIQUE KEY constraint");
    }

    private static bool IsDuplicateAssignmentException(DbUpdateException exception)
    {
        return ContainsDatabaseMessage(exception, "UQ_Inscripciones_Equipo_Jugador")
            || ContainsDatabaseMessage(exception, "Cannot insert duplicate key")
            || ContainsDatabaseMessage(exception, "Violation of UNIQUE KEY constraint");
    }

    private static bool IsTeamFullException(DbUpdateException exception)
    {
        return ContainsDatabaseMessage(exception, "TRG_Maximo12JugadoresPorEquipo")
            || ContainsDatabaseMessage(exception, "Un equipo no puede tener mas de 12 jugadores activos");
    }

    private static bool ContainsDatabaseMessage(DbUpdateException exception, string value)
    {
        var message = exception.InnerException?.Message ?? exception.Message;
        return message.Contains(value, StringComparison.OrdinalIgnoreCase);
    }
}
