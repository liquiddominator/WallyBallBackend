using Microsoft.EntityFrameworkCore;
using WallyBallBackend.Application.Jugadores;
using WallyBallBackend.Domain.Entities;
using WallyBallBackend.Infrastructure.Persistence.SqlServer;
using WallyBallBackend.Infrastructure.Personas;

namespace WallyBallBackend.Infrastructure.Jugadores;

public sealed class JugadorService : IJugadorService
{
    private const string EstadoCampeonatoActivo = "ACTIVO";
    private const string EstadoInscripcionActiva = "ACTIVO";
    private const int MaxJugadoresActivosPorEquipo = 12;

    private readonly AppDbContext _dbContext;
    private readonly IPersonasServiceClient _personasServiceClient;

    public JugadorService(AppDbContext dbContext, IPersonasServiceClient personasServiceClient)
    {
        _dbContext = dbContext;
        _personasServiceClient = personasServiceClient;
    }

    public async Task<IReadOnlyCollection<JugadorResponse>> GetJugadoresAsync(
        string? termino,
        string? cedula,
        int? equipoId,
        CancellationToken cancellationToken)
    {
        HashSet<int>? filteredPersonaIds = null;

        if (!string.IsNullOrWhiteSpace(termino) || !string.IsNullOrWhiteSpace(cedula))
        {
            var personasResult = await _personasServiceClient.GetPersonasAsync([], termino, cedula, cancellationToken);

            if (!personasResult.Succeeded)
            {
                return [];
            }

            filteredPersonaIds = personasResult.Value!
                .Select(persona => persona.IdPersona)
                .ToHashSet();
        }

        var query = CreateJugadorQuery().AsNoTracking();

        if (filteredPersonaIds is not null)
        {
            query = query.Where(jugador => jugador.IdPersona.HasValue && filteredPersonaIds.Contains(jugador.IdPersona.Value));
        }

        if (equipoId.HasValue)
        {
            query = query.Where(jugador =>
                jugador.Inscripciones.Any(inscripcion =>
                    inscripcion.IdEquipo == equipoId.Value
                    && inscripcion.Estado == EstadoInscripcionActiva));
        }

        var jugadores = await query
            .OrderBy(jugador => jugador.IdJugador)
            .ToListAsync(cancellationToken);

        return await CreateJugadorResponsesAsync(jugadores, cancellationToken);
    }

    public async Task<JugadorResponse?> GetJugadorByIdAsync(int jugadorId, CancellationToken cancellationToken)
    {
        var jugador = await CreateJugadorQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync(jugador => jugador.IdJugador == jugadorId, cancellationToken);

        if (jugador is null)
        {
            return null;
        }

        var responses = await CreateJugadorResponsesAsync([jugador], cancellationToken);

        return responses.SingleOrDefault();
    }

    public async Task<IReadOnlyCollection<JugadorEquipoResponse>> GetJugadoresByEquipoAsync(
        int equipoId,
        CancellationToken cancellationToken)
    {
        var inscripciones = await _dbContext.InscripcionesEquipoJugador
            .AsNoTracking()
            .Include(inscripcion => inscripcion.Equipo)
            .ThenInclude(equipo => equipo!.CampeonatoCategoria)
            .ThenInclude(campeonatoCategoria => campeonatoCategoria!.Campeonato)
            .Include(inscripcion => inscripcion.Equipo)
            .ThenInclude(equipo => equipo!.CampeonatoCategoria)
            .ThenInclude(campeonatoCategoria => campeonatoCategoria!.Categoria)
            .Include(inscripcion => inscripcion.Jugador)
            .Where(inscripcion => inscripcion.IdEquipo == equipoId && inscripcion.Estado == EstadoInscripcionActiva)
            .OrderBy(inscripcion => inscripcion.IdInscripcion)
            .ToListAsync(cancellationToken);

        return await CreateJugadorEquipoResponsesAsync(inscripciones, cancellationToken);
    }

    public async Task<JugadorOperationResult> CreateJugadorAsync(
        CreateJugadorRequest request,
        CancellationToken cancellationToken)
    {
        var personaResult = await _personasServiceClient.CreateJugadorAsync(
            new CreateJugadorPersonaRequest(
                request.Cedula,
                request.Nombre,
                request.Apellido,
                NormalizeOptionalText(request.Telefono),
                request.FechaNacimiento,
                request.Email,
                request.PasswordTemporal),
            cancellationToken);

        if (!personaResult.Succeeded)
        {
            return JugadorOperationResult.Failure(
                personaResult.ErrorCode ?? "personas_error",
                personaResult.ErrorMessage ?? "No se pudo crear la persona del jugador.");
        }

        var persona = personaResult.Value!;
        var alreadyLinked = await _dbContext.Jugadores
            .AnyAsync(jugador => jugador.IdPersona == persona.IdPersona, cancellationToken);

        if (alreadyLinked)
        {
            return JugadorOperationResult.Failure("duplicate_player", "La persona ya esta vinculada a un jugador.");
        }

        var jugador = new Jugador
        {
            IdPersona = persona.IdPersona,
            Activo = true
        };

        _dbContext.Jugadores.Add(jugador);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicatePlayerException(exception))
        {
            return JugadorOperationResult.Failure("duplicate_player", "La persona ya esta vinculada a un jugador.");
        }
        catch (DbUpdateException)
        {
            return JugadorOperationResult.Failure("player_persistence_error", "No se pudo registrar el jugador en la base de datos.");
        }

        return JugadorOperationResult.Success(CreateJugadorResponse(jugador, persona));
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

        var responses = await CreateJugadorEquipoResponsesAsync([inscripcion], cancellationToken);

        return JugadorOperationResult.Success(responses.Single());
    }

    private IQueryable<Jugador> CreateJugadorQuery()
    {
        return _dbContext.Jugadores
            .Include(jugador => jugador.Inscripciones)
            .ThenInclude(inscripcion => inscripcion.Equipo)
            .ThenInclude(equipo => equipo!.CampeonatoCategoria)
            .ThenInclude(campeonatoCategoria => campeonatoCategoria!.Campeonato)
            .Include(jugador => jugador.Inscripciones)
            .ThenInclude(inscripcion => inscripcion.Equipo)
            .ThenInclude(equipo => equipo!.CampeonatoCategoria)
            .ThenInclude(campeonatoCategoria => campeonatoCategoria!.Categoria);
    }

    private async Task<IReadOnlyCollection<JugadorResponse>> CreateJugadorResponsesAsync(
        IReadOnlyCollection<Jugador> jugadores,
        CancellationToken cancellationToken)
    {
        var personas = await GetPersonasByIdsAsync(
            jugadores
                .Where(jugador => jugador.IdPersona.HasValue)
                .Select(jugador => jugador.IdPersona!.Value),
            cancellationToken);

        return jugadores
            .Select(jugador => CreateJugadorResponse(
                jugador,
                jugador.IdPersona.HasValue ? personas.GetValueOrDefault(jugador.IdPersona.Value) : null))
            .OrderBy(jugador => jugador.Apellido)
            .ThenBy(jugador => jugador.Nombre)
            .ThenBy(jugador => jugador.IdJugador)
            .ToList();
    }

    private async Task<IReadOnlyCollection<JugadorEquipoResponse>> CreateJugadorEquipoResponsesAsync(
        IReadOnlyCollection<InscripcionEquipoJugador> inscripciones,
        CancellationToken cancellationToken)
    {
        var personas = await GetPersonasByIdsAsync(
            inscripciones
                .Where(inscripcion => inscripcion.Jugador?.IdPersona is not null)
                .Select(inscripcion => inscripcion.Jugador!.IdPersona!.Value),
            cancellationToken);

        return inscripciones
            .Select(inscripcion => CreateJugadorEquipoResponse(
                inscripcion,
                inscripcion.Jugador?.IdPersona is null ? null : personas.GetValueOrDefault(inscripcion.Jugador.IdPersona.Value)))
            .OrderBy(inscripcion => inscripcion.Apellido)
            .ThenBy(inscripcion => inscripcion.Nombre)
            .ThenBy(inscripcion => inscripcion.IdInscripcion)
            .ToList();
    }

    private async Task<Dictionary<int, PersonaClientResponse>> GetPersonasByIdsAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken)
    {
        var distinctIds = ids.Distinct().ToArray();

        if (distinctIds.Length == 0)
        {
            return [];
        }

        var result = await _personasServiceClient.GetPersonasAsync(distinctIds, null, null, cancellationToken);

        return result.Succeeded
            ? result.Value!.ToDictionary(persona => persona.IdPersona)
            : [];
    }

    private static JugadorResponse CreateJugadorResponse(
        Jugador jugador,
        JugadorPersonaClientResponse persona)
    {
        return CreateJugadorResponse(
            jugador,
            new PersonaClientResponse(
                persona.IdPersona,
                persona.Cedula,
                persona.Nombre,
                persona.Apellido,
                persona.Telefono,
                persona.FechaNacimiento,
                true),
            persona.IdUsuario,
            persona.Email);
    }

    private static JugadorResponse CreateJugadorResponse(
        Jugador jugador,
        PersonaClientResponse? persona,
        int? idUsuario = null,
        string? email = null)
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
            jugador.IdPersona ?? 0,
            idUsuario,
            persona?.Cedula ?? string.Empty,
            persona?.Nombre ?? string.Empty,
            persona?.Apellido ?? string.Empty,
            email,
            persona?.Telefono,
            persona?.FechaNacimiento,
            jugador.Activo,
            jugador.FechaCreacion,
            jugador.FechaActualizacion,
            equipos);
    }

    private static JugadorEquipoResponse CreateJugadorEquipoResponse(
        InscripcionEquipoJugador inscripcion,
        PersonaClientResponse? persona)
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
            persona?.Cedula ?? string.Empty,
            persona?.Nombre ?? string.Empty,
            persona?.Apellido ?? string.Empty,
            persona?.Telefono,
            persona?.FechaNacimiento,
            inscripcion.Estado,
            inscripcion.FechaInscripcion);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool IsDuplicatePlayerException(DbUpdateException exception)
    {
        return ContainsDatabaseMessage(exception, "UQ_Jugadores_IdPersona")
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
