using Microsoft.EntityFrameworkCore;
using WallyBallBackend.Application.Fixture;
using WallyBallBackend.Domain.Entities;
using WallyBallBackend.Infrastructure.Persistence.SqlServer;

namespace WallyBallBackend.Infrastructure.Fixture;

public sealed class FixtureService : IFixtureService
{
    private const string EstadoCampeonatoActivo = "ACTIVO";
    private const string EstadoCampeonatoCategoriaActiva = "ACTIVA";
    private const string EstadoEquipoActivo = "ACTIVO";
    private const string EstadoFaseActiva = "ACTIVA";
    private const string EstadoPartidoProgramado = "PROGRAMADO";
    private const string EstadoPartidoReprogramado = "REPROGRAMADO";
    private const string EstadoPartidoFinalizado = "FINALIZADO";
    private const string EstadoPartidoCancelado = "CANCELADO";
    private const string TipoRoundRobin = "ROUND_ROBIN";

    private readonly AppDbContext _dbContext;

    public FixtureService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FixtureResponse?> GetFixtureAsync(int campeonatoCategoriaId, CancellationToken cancellationToken)
    {
        var campeonatoCategoria = await _dbContext.CampeonatoCategorias
            .AsNoTracking()
            .Include(campeonatoCategoria => campeonatoCategoria.Campeonato)
            .Include(campeonatoCategoria => campeonatoCategoria.Categoria)
            .Include(campeonatoCategoria => campeonatoCategoria.Fases)
            .ThenInclude(fase => fase.Jornadas)
            .ThenInclude(jornada => jornada.Partidos)
            .ThenInclude(partido => partido.EquipoLocal)
            .Include(campeonatoCategoria => campeonatoCategoria.Fases)
            .ThenInclude(fase => fase.Jornadas)
            .ThenInclude(jornada => jornada.Partidos)
            .ThenInclude(partido => partido.EquipoVisitante)
            .Include(campeonatoCategoria => campeonatoCategoria.Fases)
            .ThenInclude(fase => fase.Jornadas)
            .ThenInclude(jornada => jornada.Partidos)
            .ThenInclude(partido => partido.Reprogramaciones)
            .SingleOrDefaultAsync(
                campeonatoCategoria => campeonatoCategoria.IdCampeonatoCategoria == campeonatoCategoriaId,
                cancellationToken);

        return campeonatoCategoria is null ? null : CreateFixtureResponse(campeonatoCategoria);
    }

    public async Task<FixtureOperationResult> GenerateFixtureAsync(
        int campeonatoCategoriaId,
        GenerateFixtureRequest request,
        CancellationToken cancellationToken)
    {
        var campeonatoCategoria = await _dbContext.CampeonatoCategorias
            .Include(campeonatoCategoria => campeonatoCategoria.Campeonato)
            .Include(campeonatoCategoria => campeonatoCategoria.Categoria)
            .Include(campeonatoCategoria => campeonatoCategoria.Fases)
            .ThenInclude(fase => fase.Jornadas)
            .ThenInclude(jornada => jornada.Partidos)
            .Include(campeonatoCategoria => campeonatoCategoria.Equipos)
            .SingleOrDefaultAsync(
                campeonatoCategoria => campeonatoCategoria.IdCampeonatoCategoria == campeonatoCategoriaId,
                cancellationToken);

        if (campeonatoCategoria is null)
        {
            return FixtureOperationResult.Failure("championship_category_not_found", "Categoria del campeonato no encontrada.");
        }

        if (campeonatoCategoria.Campeonato is null
            || !string.Equals(campeonatoCategoria.Campeonato.Estado, EstadoCampeonatoActivo, StringComparison.OrdinalIgnoreCase))
        {
            return FixtureOperationResult.Failure("inactive_championship", "No se puede generar fixture para un campeonato finalizado o inactivo.");
        }

        if (!string.Equals(campeonatoCategoria.Estado, EstadoCampeonatoCategoriaActiva, StringComparison.OrdinalIgnoreCase))
        {
            return FixtureOperationResult.Failure("inactive_championship_category", "La categoria del campeonato no esta activa.");
        }

        var hasFixture = campeonatoCategoria.Fases
            .Any(fase => !string.Equals(fase.Estado, "CANCELADA", StringComparison.OrdinalIgnoreCase));

        if (hasFixture)
        {
            return FixtureOperationResult.Failure("fixture_already_exists", "Ya existe fixture generado para esta categoria del campeonato.");
        }

        var equipos = campeonatoCategoria.Equipos
            .Where(equipo => equipo.Activo)
            .OrderBy(equipo => equipo.IdEquipo)
            .ToList();

        if (equipos.Count < 2)
        {
            return FixtureOperationResult.Failure("not_enough_teams", "Se requieren al menos dos equipos activos para generar el fixture.");
        }

        var fase = new Fase
        {
            IdCampeonatoCategoria = campeonatoCategoriaId,
            CampeonatoCategoria = campeonatoCategoria,
            Nombre = "Todos contra todos",
            Tipo = TipoRoundRobin,
            Orden = 1,
            Estado = EstadoFaseActiva
        };

        var jornadas = CreateRoundRobinJornadas(fase, campeonatoCategoriaId, equipos, request);

        foreach (var jornada in jornadas)
        {
            fase.Jornadas.Add(jornada);
        }

        _dbContext.Fases.Add(fase);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicateFixtureException(exception))
        {
            return FixtureOperationResult.Failure("fixture_already_exists", "Ya existe fixture generado para esta categoria del campeonato.");
        }
        catch (DbUpdateException exception) when (IsInvalidFixtureException(exception))
        {
            return FixtureOperationResult.Failure("invalid_fixture", "El fixture generado no cumple las reglas de integridad.");
        }
        catch (DbUpdateException)
        {
            return FixtureOperationResult.Failure("fixture_persistence_error", "No se pudo generar el fixture en la base de datos.");
        }

        return FixtureOperationResult.Success(CreateFixtureResponse(campeonatoCategoria));
    }

    public async Task<FixtureOperationResult> ReprogramarPartidoAsync(
        int partidoId,
        ReprogramarPartidoRequest request,
        CancellationToken cancellationToken)
    {
        var partido = await _dbContext.Partidos
            .Include(partido => partido.CampeonatoCategoria)
            .ThenInclude(campeonatoCategoria => campeonatoCategoria!.Campeonato)
            .Include(partido => partido.Fase)
            .Include(partido => partido.Jornada)
            .Include(partido => partido.EquipoLocal)
            .Include(partido => partido.EquipoVisitante)
            .Include(partido => partido.Reprogramaciones)
            .SingleOrDefaultAsync(partido => partido.IdPartido == partidoId, cancellationToken);

        if (partido is null)
        {
            return FixtureOperationResult.Failure("match_not_found", "Partido no encontrado.");
        }

        if (partido.CampeonatoCategoria?.Campeonato is null
            || !string.Equals(partido.CampeonatoCategoria.Campeonato.Estado, EstadoCampeonatoActivo, StringComparison.OrdinalIgnoreCase))
        {
            return FixtureOperationResult.Failure("inactive_championship", "No se puede reprogramar un partido de un campeonato finalizado o inactivo.");
        }

        if (string.Equals(partido.Estado, EstadoPartidoFinalizado, StringComparison.OrdinalIgnoreCase)
            || string.Equals(partido.Estado, EstadoPartidoCancelado, StringComparison.OrdinalIgnoreCase))
        {
            return FixtureOperationResult.Failure("match_not_reprogrammable", "No se puede reprogramar un partido finalizado o cancelado.");
        }

        var reprogramacion = new ReprogramacionPartido
        {
            IdPartido = partido.IdPartido,
            Partido = partido,
            FechaHoraAnterior = partido.FechaHoraProgramada,
            FechaHoraNueva = request.FechaHoraNueva,
            Motivo = NormalizeOptionalText(request.Motivo),
            FechaRegistro = DateTime.UtcNow
        };

        partido.FechaHoraProgramada = request.FechaHoraNueva;
        partido.Estado = EstadoPartidoReprogramado;
        partido.Reprogramaciones.Add(reprogramacion);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return FixtureOperationResult.Failure("match_persistence_error", "No se pudo reprogramar el partido.");
        }

        return FixtureOperationResult.Success(CreatePartidoResponse(partido));
    }

    private static IReadOnlyCollection<Jornada> CreateRoundRobinJornadas(
        Fase fase,
        int campeonatoCategoriaId,
        IReadOnlyCollection<Equipo> equipos,
        GenerateFixtureRequest request)
    {
        var equiposRotacion = equipos.Cast<Equipo?>().ToList();

        if (equiposRotacion.Count % 2 != 0)
        {
            equiposRotacion.Add(null);
        }

        var totalEquipos = equiposRotacion.Count;
        var totalJornadas = totalEquipos - 1;
        var primeraFecha = request.FechaPrimeraJornada;
        var diasEntreJornadas = request.DiasEntreJornadas <= 0 ? 7 : request.DiasEntreJornadas;
        var horaPartidos = request.HoraPartidos;
        var jornadas = new List<Jornada>();

        for (var numeroJornada = 1; numeroJornada <= totalJornadas; numeroJornada++)
        {
            var fechaJornada = primeraFecha?.AddDays((numeroJornada - 1) * diasEntreJornadas);
            var jornada = new Jornada
            {
                Fase = fase,
                NumeroJornada = numeroJornada,
                FechaProgramada = fechaJornada,
                Estado = "PROGRAMADA"
            };

            for (var index = 0; index < totalEquipos / 2; index++)
            {
                var local = equiposRotacion[index];
                var visitante = equiposRotacion[totalEquipos - 1 - index];

                if (local is null || visitante is null)
                {
                    continue;
                }

                if (numeroJornada % 2 == 0)
                {
                    (local, visitante) = (visitante, local);
                }

                jornada.Partidos.Add(new Partido
                {
                    IdCampeonatoCategoria = campeonatoCategoriaId,
                    CampeonatoCategoria = fase.CampeonatoCategoria,
                    Fase = fase,
                    Jornada = jornada,
                    IdEquipoLocal = local.IdEquipo,
                    EquipoLocal = local,
                    IdEquipoVisitante = visitante.IdEquipo,
                    EquipoVisitante = visitante,
                    FechaHoraProgramada = fechaJornada.HasValue && horaPartidos.HasValue
                        ? fechaJornada.Value.ToDateTime(horaPartidos.Value)
                        : null,
                    Estado = EstadoPartidoProgramado
                });
            }

            jornadas.Add(jornada);

            var last = equiposRotacion[^1];
            equiposRotacion.RemoveAt(equiposRotacion.Count - 1);
            equiposRotacion.Insert(1, last);
        }

        return jornadas;
    }

    private static FixtureResponse CreateFixtureResponse(CampeonatoCategoria campeonatoCategoria)
    {
        var fases = campeonatoCategoria.Fases
            .OrderBy(fase => fase.Orden)
            .ThenBy(fase => fase.IdFase)
            .Select(fase => new FaseFixtureResponse(
                fase.IdFase,
                fase.Nombre,
                fase.Tipo,
                fase.Orden,
                fase.Estado,
                fase.Jornadas
                    .OrderBy(jornada => jornada.NumeroJornada)
                    .ThenBy(jornada => jornada.IdJornada)
                    .Select(jornada => new JornadaFixtureResponse(
                        jornada.IdJornada,
                        jornada.NumeroJornada,
                        jornada.FechaProgramada,
                        jornada.Estado,
                        jornada.Partidos
                            .OrderBy(partido => partido.IdPartido)
                            .Select(CreatePartidoResponse)
                            .ToList()))
                    .ToList()))
            .ToList();

        return new FixtureResponse(
            campeonatoCategoria.IdCampeonatoCategoria,
            campeonatoCategoria.IdCampeonato,
            campeonatoCategoria.Campeonato?.Nombre ?? string.Empty,
            campeonatoCategoria.IdCategoria,
            campeonatoCategoria.Categoria?.Nombre ?? string.Empty,
            fases);
    }

    private static PartidoResponse CreatePartidoResponse(Partido partido)
    {
        return new PartidoResponse(
            partido.IdPartido,
            partido.IdCampeonatoCategoria,
            partido.IdFase,
            partido.Fase?.Nombre ?? string.Empty,
            partido.IdJornada,
            partido.Jornada?.NumeroJornada ?? 0,
            partido.Jornada?.FechaProgramada,
            partido.IdEquipoLocal,
            partido.EquipoLocal?.Nombre ?? string.Empty,
            partido.IdEquipoVisitante,
            partido.EquipoVisitante?.Nombre ?? string.Empty,
            partido.FechaHoraProgramada,
            partido.Estado,
            partido.Reprogramaciones
                .OrderBy(reprogramacion => reprogramacion.FechaRegistro)
                .Select(reprogramacion => new ReprogramacionPartidoResponse(
                    reprogramacion.IdReprogramacion,
                    reprogramacion.FechaHoraAnterior,
                    reprogramacion.FechaHoraNueva,
                    reprogramacion.Motivo,
                    reprogramacion.FechaRegistro))
                .ToList());
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool IsDuplicateFixtureException(DbUpdateException exception)
    {
        return ContainsDatabaseMessage(exception, "UQ_Fases_CampeonatoCategoria_Orden")
            || ContainsDatabaseMessage(exception, "UQ_Partidos_Fase_Enfrentamiento")
            || ContainsDatabaseMessage(exception, "Cannot insert duplicate key")
            || ContainsDatabaseMessage(exception, "Violation of UNIQUE KEY constraint");
    }

    private static bool IsInvalidFixtureException(DbUpdateException exception)
    {
        return ContainsDatabaseMessage(exception, "TRG_Partidos_ValidarIntegridad")
            || ContainsDatabaseMessage(exception, "Un equipo no puede jugar mas de una vez en la misma jornada")
            || ContainsDatabaseMessage(exception, "IdEquipoLocal <> IdEquipoVisitante");
    }

    private static bool ContainsDatabaseMessage(DbUpdateException exception, string value)
    {
        var message = exception.InnerException?.Message ?? exception.Message;
        return message.Contains(value, StringComparison.OrdinalIgnoreCase);
    }
}
