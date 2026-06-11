using Microsoft.EntityFrameworkCore;
using WallyBallBackend.Application.Resultados;
using WallyBallBackend.Domain.Entities;
using WallyBallBackend.Infrastructure.Persistence.SqlServer;

namespace WallyBallBackend.Infrastructure.Resultados;

public sealed class ResultadoService : IResultadoService
{
    private const string EstadoCampeonatoActivo = "ACTIVO";
    private const string EstadoPartidoCancelado = "CANCELADO";
    private const string EstadoPartidoFinalizado = "FINALIZADO";

    private readonly AppDbContext _dbContext;

    public ResultadoService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ResultadoResponse>> GetResultadosAsync(
        int? campeonatoCategoriaId,
        CancellationToken cancellationToken)
    {
        var query = CreateResultadoQuery().AsNoTracking();

        if (campeonatoCategoriaId.HasValue)
        {
            query = query.Where(resultado => resultado.Partido!.IdCampeonatoCategoria == campeonatoCategoriaId.Value);
        }

        var resultados = await query
            .OrderByDescending(resultado => resultado.FechaRegistro)
            .ThenByDescending(resultado => resultado.IdResultado)
            .ToListAsync(cancellationToken);

        return resultados.Select(CreateResultadoResponse).ToList();
    }

    public async Task<ResultadoResponse?> GetResultadoByIdAsync(int resultadoId, CancellationToken cancellationToken)
    {
        var resultado = await CreateResultadoQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync(resultado => resultado.IdResultado == resultadoId, cancellationToken);

        return resultado is null ? null : CreateResultadoResponse(resultado);
    }

    public async Task<ResultadoResponse?> GetResultadoByPartidoAsync(int partidoId, CancellationToken cancellationToken)
    {
        var resultado = await CreateResultadoQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync(resultado => resultado.IdPartido == partidoId, cancellationToken);

        return resultado is null ? null : CreateResultadoResponse(resultado);
    }

    public async Task<IReadOnlyCollection<AuditoriaResultadoResponse>> GetAuditoriaResultadoAsync(
        int resultadoId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.AuditoriaResultados
            .AsNoTracking()
            .Where(auditoria => auditoria.IdResultado == resultadoId)
            .OrderByDescending(auditoria => auditoria.FechaCambio)
            .ThenByDescending(auditoria => auditoria.IdAuditoriaResultado)
            .Select(auditoria => new AuditoriaResultadoResponse(
                auditoria.IdAuditoriaResultado,
                auditoria.IdResultado,
                auditoria.IdPartido,
                auditoria.SetsLocalAnterior,
                auditoria.SetsVisitanteAnterior,
                auditoria.IdEquipoGanadorAnterior,
                auditoria.SetsLocalNuevo,
                auditoria.SetsVisitanteNuevo,
                auditoria.IdEquipoGanadorNuevo,
                auditoria.Motivo,
                auditoria.FechaCambio))
            .ToListAsync(cancellationToken);
    }

    public async Task<ResultadoOperationResult> RegisterResultadoAsync(
        int partidoId,
        RegisterResultadoRequest request,
        CancellationToken cancellationToken)
    {
        var partido = await CreatePartidoQuery()
            .SingleOrDefaultAsync(partido => partido.IdPartido == partidoId, cancellationToken);

        if (partido is null)
        {
            return ResultadoOperationResult.Failure("match_not_found", "Partido no encontrado.");
        }

        var validationFailure = ValidatePartidoForResult(partido);

        if (validationFailure is not null)
        {
            return validationFailure;
        }

        if (partido.Resultado is not null)
        {
            return ResultadoOperationResult.Failure("result_already_exists", "El partido ya tiene resultado registrado.");
        }

        var aggregate = CalculateAggregate(partido, request.Sets);
        var resultado = new Resultado
        {
            IdPartido = partido.IdPartido,
            Partido = partido,
            SetsLocal = aggregate.SetsLocal,
            SetsVisitante = aggregate.SetsVisitante,
            IdEquipoGanador = aggregate.IdEquipoGanador,
            FechaRegistro = DateTime.UtcNow,
            Sets = CreateResultadoSets(request.Sets)
        };

        partido.Resultado = resultado;
        partido.Estado = EstadoPartidoFinalizado;
        _dbContext.Resultados.Add(resultado);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicateResultException(exception))
        {
            return ResultadoOperationResult.Failure("result_already_exists", "El partido ya tiene resultado registrado.");
        }
        catch (DbUpdateException exception) when (IsInactiveChampionshipException(exception))
        {
            return ResultadoOperationResult.Failure("inactive_championship", "No se pueden registrar resultados en un campeonato finalizado o inactivo.");
        }
        catch (DbUpdateException exception) when (IsInvalidResultException(exception))
        {
            return ResultadoOperationResult.Failure("invalid_result", "El resultado no cumple las reglas de integridad.");
        }
        catch (DbUpdateException)
        {
            return ResultadoOperationResult.Failure("result_persistence_error", "No se pudo registrar el resultado.");
        }

        var created = await GetResultadoByIdAsync(resultado.IdResultado, cancellationToken);

        return ResultadoOperationResult.Success(created!);
    }

    public async Task<ResultadoOperationResult> UpdateResultadoAsync(
        int resultadoId,
        UpdateResultadoRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await CreateResultadoQuery()
            .SingleOrDefaultAsync(resultado => resultado.IdResultado == resultadoId, cancellationToken);

        if (resultado is null)
        {
            return ResultadoOperationResult.Failure("result_not_found", "Resultado no encontrado.");
        }

        if (resultado.Partido is null)
        {
            return ResultadoOperationResult.Failure("match_not_found", "Partido no encontrado.");
        }

        var validationFailure = ValidatePartidoForResult(resultado.Partido);

        if (validationFailure is not null)
        {
            return validationFailure;
        }

        var previousSetsLocal = resultado.SetsLocal;
        var previousSetsVisitante = resultado.SetsVisitante;
        var previousWinner = resultado.IdEquipoGanador;
        var aggregate = CalculateAggregate(resultado.Partido, request.Sets);
        var consolidatedChanged = previousSetsLocal != aggregate.SetsLocal
            || previousSetsVisitante != aggregate.SetsVisitante
            || previousWinner != aggregate.IdEquipoGanador;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var currentSets = resultado.Sets.ToList();
            _dbContext.ResultadoSets.RemoveRange(currentSets);
            await _dbContext.SaveChangesAsync(cancellationToken);
            resultado.Sets.Clear();

            resultado.SetsLocal = aggregate.SetsLocal;
            resultado.SetsVisitante = aggregate.SetsVisitante;
            resultado.IdEquipoGanador = aggregate.IdEquipoGanador;
            resultado.FechaActualizacion = DateTime.UtcNow;

            foreach (var set in CreateResultadoSets(request.Sets))
            {
                resultado.Sets.Add(set);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await EnsureAuditMotivoAsync(
                resultado,
                previousSetsLocal,
                previousSetsVisitante,
                previousWinner,
                request.Motivo,
                consolidatedChanged,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsInactiveChampionshipException(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            return ResultadoOperationResult.Failure("inactive_championship", "No se pueden modificar resultados de un campeonato finalizado o inactivo.");
        }
        catch (DbUpdateException exception) when (IsInvalidResultException(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            return ResultadoOperationResult.Failure("invalid_result", "El resultado no cumple las reglas de integridad.");
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return ResultadoOperationResult.Failure("result_persistence_error", "No se pudo modificar el resultado.");
        }

        var updated = await GetResultadoByIdAsync(resultado.IdResultado, cancellationToken);

        return ResultadoOperationResult.Success(updated!);
    }

    private IQueryable<Resultado> CreateResultadoQuery()
    {
        return _dbContext.Resultados
            .Include(resultado => resultado.Partido)
            .ThenInclude(partido => partido!.Fase)
            .Include(resultado => resultado.Partido)
            .ThenInclude(partido => partido!.Jornada)
            .Include(resultado => resultado.Partido)
            .ThenInclude(partido => partido!.EquipoLocal)
            .Include(resultado => resultado.Partido)
            .ThenInclude(partido => partido!.EquipoVisitante)
            .Include(resultado => resultado.Partido)
            .ThenInclude(partido => partido!.CampeonatoCategoria)
            .ThenInclude(campeonatoCategoria => campeonatoCategoria!.Campeonato)
            .Include(resultado => resultado.EquipoGanador)
            .Include(resultado => resultado.Sets);
    }

    private IQueryable<Partido> CreatePartidoQuery()
    {
        return _dbContext.Partidos
            .Include(partido => partido.CampeonatoCategoria)
            .ThenInclude(campeonatoCategoria => campeonatoCategoria!.Campeonato)
            .Include(partido => partido.EquipoLocal)
            .Include(partido => partido.EquipoVisitante)
            .Include(partido => partido.Resultado);
    }

    private static ResultadoOperationResult? ValidatePartidoForResult(Partido partido)
    {
        if (partido.CampeonatoCategoria?.Campeonato is null
            || !string.Equals(partido.CampeonatoCategoria.Campeonato.Estado, EstadoCampeonatoActivo, StringComparison.OrdinalIgnoreCase))
        {
            return ResultadoOperationResult.Failure("inactive_championship", "No se pueden registrar o modificar resultados en un campeonato finalizado o inactivo.");
        }

        if (string.Equals(partido.Estado, EstadoPartidoCancelado, StringComparison.OrdinalIgnoreCase))
        {
            return ResultadoOperationResult.Failure("match_cancelled", "No se puede registrar resultado para un partido cancelado.");
        }

        return null;
    }

    private static ResultadoAggregate CalculateAggregate(
        Partido partido,
        IReadOnlyCollection<ResultadoSetRequest> sets)
    {
        var setsLocal = sets.Count(set => set.PuntosLocal > set.PuntosVisitante);
        var setsVisitante = sets.Count(set => set.PuntosVisitante > set.PuntosLocal);
        var idEquipoGanador = setsLocal > setsVisitante
            ? partido.IdEquipoLocal
            : partido.IdEquipoVisitante;

        return new ResultadoAggregate(setsLocal, setsVisitante, idEquipoGanador);
    }

    private static List<ResultadoSet> CreateResultadoSets(IReadOnlyCollection<ResultadoSetRequest> sets)
    {
        return sets
            .OrderBy(set => set.NumeroSet)
            .Select(set => new ResultadoSet
            {
                NumeroSet = set.NumeroSet,
                PuntosLocal = set.PuntosLocal,
                PuntosVisitante = set.PuntosVisitante
            })
            .ToList();
    }

    private async Task EnsureAuditMotivoAsync(
        Resultado resultado,
        int previousSetsLocal,
        int previousSetsVisitante,
        int previousWinner,
        string? motivo,
        bool consolidatedChanged,
        CancellationToken cancellationToken)
    {
        var normalizedMotivo = NormalizeOptionalText(motivo);

        if (consolidatedChanged)
        {
            var latestAudit = await _dbContext.AuditoriaResultados
                .Where(auditoria => auditoria.IdResultado == resultado.IdResultado)
                .OrderByDescending(auditoria => auditoria.FechaCambio)
                .ThenByDescending(auditoria => auditoria.IdAuditoriaResultado)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestAudit is not null && normalizedMotivo is not null)
            {
                latestAudit.Motivo = normalizedMotivo;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return;
        }

        var audit = new AuditoriaResultado
        {
            IdResultado = resultado.IdResultado,
            IdPartido = resultado.IdPartido,
            SetsLocalAnterior = previousSetsLocal,
            SetsVisitanteAnterior = previousSetsVisitante,
            IdEquipoGanadorAnterior = previousWinner,
            SetsLocalNuevo = resultado.SetsLocal,
            SetsVisitanteNuevo = resultado.SetsVisitante,
            IdEquipoGanadorNuevo = resultado.IdEquipoGanador,
            Motivo = normalizedMotivo,
            FechaCambio = DateTime.UtcNow
        };

        _dbContext.AuditoriaResultados.Add(audit);
        await _dbContext.SaveChangesAsync(cancellationToken);
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

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool IsDuplicateResultException(DbUpdateException exception)
    {
        return ContainsDatabaseMessage(exception, "UQ_Resultados_Partido")
            || ContainsDatabaseMessage(exception, "Cannot insert duplicate key")
            || ContainsDatabaseMessage(exception, "Violation of UNIQUE KEY constraint");
    }

    private static bool IsInactiveChampionshipException(DbUpdateException exception)
    {
        return ContainsDatabaseMessage(exception, "No se pueden registrar o modificar resultados de un campeonato finalizado");
    }

    private static bool IsInvalidResultException(DbUpdateException exception)
    {
        return ContainsDatabaseMessage(exception, "CK_Resultados")
            || ContainsDatabaseMessage(exception, "CK_ResultadoSets")
            || ContainsDatabaseMessage(exception, "El equipo ganador debe")
            || ContainsDatabaseMessage(exception, "no puede terminar empatado");
    }

    private static bool ContainsDatabaseMessage(DbUpdateException exception, string value)
    {
        var message = exception.InnerException?.Message ?? exception.Message;
        return message.Contains(value, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record ResultadoAggregate(
        int SetsLocal,
        int SetsVisitante,
        int IdEquipoGanador);
}
