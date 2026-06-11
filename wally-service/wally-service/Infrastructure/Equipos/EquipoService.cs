using Microsoft.EntityFrameworkCore;
using WallyBallBackend.Application.Equipos;
using WallyBallBackend.Domain.Entities;
using WallyBallBackend.Infrastructure.Persistence.SqlServer;

namespace WallyBallBackend.Infrastructure.Equipos;

public sealed class EquipoService : IEquipoService
{
    private const string EstadoCampeonatoActivo = "ACTIVO";
    private const string EstadoInscripcionActiva = "ACTIVO";

    private readonly AppDbContext _dbContext;

    public EquipoService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<EquipoResponse>> GetEquiposAsync(
        int? campeonatoCategoriaId,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Equipos
            .AsNoTracking();

        if (campeonatoCategoriaId.HasValue)
        {
            query = query.Where(equipo => equipo.IdCampeonatoCategoria == campeonatoCategoriaId.Value);
        }

        return await query
            .OrderBy(equipo => equipo.CampeonatoCategoria!.Campeonato!.Nombre)
            .ThenBy(equipo => equipo.CampeonatoCategoria!.Categoria!.Nombre)
            .ThenBy(equipo => equipo.Nombre)
            .ThenBy(equipo => equipo.IdEquipo)
            .Select(equipo => CreateResponse(
                equipo,
                equipo.CampeonatoCategoria!.IdCategoria,
                equipo.CampeonatoCategoria.Categoria!.Nombre,
                equipo.CampeonatoCategoria.IdCampeonato,
                equipo.CampeonatoCategoria.Campeonato!.Nombre,
                equipo.Inscripciones.Count(inscripcion => inscripcion.Estado == EstadoInscripcionActiva)))
            .ToListAsync(cancellationToken);
    }

    public async Task<EquipoResponse?> GetEquipoByIdAsync(int equipoId, CancellationToken cancellationToken)
    {
        return await _dbContext.Equipos
            .AsNoTracking()
            .Where(equipo => equipo.IdEquipo == equipoId)
            .Select(equipo => CreateResponse(
                equipo,
                equipo.CampeonatoCategoria!.IdCategoria,
                equipo.CampeonatoCategoria.Categoria!.Nombre,
                equipo.CampeonatoCategoria.IdCampeonato,
                equipo.CampeonatoCategoria.Campeonato!.Nombre,
                equipo.Inscripciones.Count(inscripcion => inscripcion.Estado == EstadoInscripcionActiva)))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<EquipoOperationResult> CreateEquipoAsync(
        int campeonatoCategoriaId,
        CreateEquipoRequest request,
        CancellationToken cancellationToken)
    {
        var nombre = request.Nombre.Trim();
        var campeonatoCategoria = await _dbContext.CampeonatoCategorias
            .Include(campeonatoCategoria => campeonatoCategoria.Campeonato)
            .Include(campeonatoCategoria => campeonatoCategoria.Categoria)
            .SingleOrDefaultAsync(
                campeonatoCategoria => campeonatoCategoria.IdCampeonatoCategoria == campeonatoCategoriaId,
                cancellationToken);

        if (campeonatoCategoria is null)
        {
            return EquipoOperationResult.Failure("championship_category_not_found", "Categoria del campeonato no encontrada.");
        }

        if (campeonatoCategoria.Campeonato is null
            || !string.Equals(campeonatoCategoria.Campeonato.Estado, EstadoCampeonatoActivo, StringComparison.OrdinalIgnoreCase))
        {
            return EquipoOperationResult.Failure("inactive_championship", "No se pueden registrar equipos en un campeonato finalizado o inactivo.");
        }

        var exists = await _dbContext.Equipos
            .AnyAsync(
                equipo => equipo.IdCampeonatoCategoria == campeonatoCategoriaId
                    && equipo.Nombre == nombre,
                cancellationToken);

        if (exists)
        {
            return EquipoOperationResult.Failure("duplicate_team", "Ya existe un equipo con ese nombre en la categoria.");
        }

        var equipo = new Equipo
        {
            IdCampeonatoCategoria = campeonatoCategoriaId,
            CampeonatoCategoria = campeonatoCategoria,
            Nombre = nombre,
            Activo = true
        };

        _dbContext.Equipos.Add(equipo);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicateTeamException(exception))
        {
            return EquipoOperationResult.Failure("duplicate_team", "Ya existe un equipo con ese nombre en la categoria.");
        }
        catch (DbUpdateException)
        {
            return EquipoOperationResult.Failure("team_persistence_error", "No se pudo registrar el equipo en la base de datos.");
        }

        return EquipoOperationResult.Success(CreateResponse(
            equipo,
            campeonatoCategoria.IdCategoria,
            campeonatoCategoria.Categoria?.Nombre ?? string.Empty,
            campeonatoCategoria.IdCampeonato,
            campeonatoCategoria.Campeonato.Nombre,
            cantidadJugadores: 0));
    }

    public async Task<EquipoOperationResult> UpdateEquipoAsync(
        int equipoId,
        UpdateEquipoRequest request,
        CancellationToken cancellationToken)
    {
        var nombre = request.Nombre.Trim();
        var equipo = await _dbContext.Equipos
            .Include(equipo => equipo.CampeonatoCategoria)
            .ThenInclude(campeonatoCategoria => campeonatoCategoria!.Campeonato)
            .Include(equipo => equipo.CampeonatoCategoria)
            .ThenInclude(campeonatoCategoria => campeonatoCategoria!.Categoria)
            .Include(equipo => equipo.Inscripciones)
            .SingleOrDefaultAsync(equipo => equipo.IdEquipo == equipoId, cancellationToken);

        if (equipo is null)
        {
            return EquipoOperationResult.Failure("not_found", "Equipo no encontrado.");
        }

        if (equipo.CampeonatoCategoria?.Campeonato is null
            || !string.Equals(equipo.CampeonatoCategoria.Campeonato.Estado, EstadoCampeonatoActivo, StringComparison.OrdinalIgnoreCase))
        {
            return EquipoOperationResult.Failure("inactive_championship", "Solo equipos de campeonatos activos pueden modificarse.");
        }

        var exists = await _dbContext.Equipos
            .AnyAsync(
                existing => existing.IdCampeonatoCategoria == equipo.IdCampeonatoCategoria
                    && existing.IdEquipo != equipoId
                    && existing.Nombre == nombre,
                cancellationToken);

        if (exists)
        {
            return EquipoOperationResult.Failure("duplicate_team", "Ya existe un equipo con ese nombre en la categoria.");
        }

        equipo.Nombre = nombre;
        equipo.FechaActualizacion = DateTime.UtcNow;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicateTeamException(exception))
        {
            return EquipoOperationResult.Failure("duplicate_team", "Ya existe un equipo con ese nombre en la categoria.");
        }
        catch (DbUpdateException)
        {
            return EquipoOperationResult.Failure("team_persistence_error", "No se pudo actualizar el equipo en la base de datos.");
        }

        return EquipoOperationResult.Success(CreateResponse(
            equipo,
            equipo.CampeonatoCategoria.IdCategoria,
            equipo.CampeonatoCategoria.Categoria?.Nombre ?? string.Empty,
            equipo.CampeonatoCategoria.IdCampeonato,
            equipo.CampeonatoCategoria.Campeonato.Nombre,
            equipo.Inscripciones.Count(inscripcion => string.Equals(inscripcion.Estado, EstadoInscripcionActiva, StringComparison.OrdinalIgnoreCase))));
    }

    private static bool IsDuplicateTeamException(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;

        return message.Contains("UQ_Equipos_CampeonatoCategoria_Nombre", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Cannot insert duplicate key", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Violation of UNIQUE KEY constraint", StringComparison.OrdinalIgnoreCase);
    }

    private static EquipoResponse CreateResponse(
        Equipo equipo,
        int idCategoria,
        string categoriaNombre,
        int idCampeonato,
        string campeonatoNombre,
        int cantidadJugadores)
    {
        return new EquipoResponse(
            equipo.IdEquipo,
            equipo.IdCampeonatoCategoria,
            idCategoria,
            categoriaNombre,
            idCampeonato,
            campeonatoNombre,
            equipo.Nombre,
            equipo.Activo,
            cantidadJugadores,
            equipo.FechaCreacion,
            equipo.FechaActualizacion);
    }
}
