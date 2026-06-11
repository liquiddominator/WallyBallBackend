using Microsoft.EntityFrameworkCore;
using WallyBallBackend.Application.Campeonatos;
using WallyBallBackend.Domain.Entities;
using WallyBallBackend.Infrastructure.Persistence.SqlServer;

namespace WallyBallBackend.Infrastructure.Campeonatos;

public sealed class CampeonatoService : ICampeonatoService
{
    private const string EstadoActivo = "ACTIVO";
    private const string EstadoFinalizado = "FINALIZADO";

    private readonly AppDbContext _dbContext;

    public CampeonatoService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<CampeonatoResponse>> GetCampeonatosAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Campeonatos
            .AsNoTracking()
            .OrderByDescending(campeonato => campeonato.FechaCreacion)
            .ThenBy(campeonato => campeonato.IdCampeonato)
            .Select(campeonato => CreateResponse(campeonato))
            .ToListAsync(cancellationToken);
    }

    public async Task<CampeonatoResponse?> GetCampeonatoByIdAsync(int campeonatoId, CancellationToken cancellationToken)
    {
        return await _dbContext.Campeonatos
            .AsNoTracking()
            .Where(campeonato => campeonato.IdCampeonato == campeonatoId)
            .Select(campeonato => CreateResponse(campeonato))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<CampeonatoOperationResult> CreateCampeonatoAsync(CreateCampeonatoRequest request, CancellationToken cancellationToken)
    {
        var campeonato = new Campeonato
        {
            Nombre = request.Nombre.Trim(),
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            Estado = EstadoActivo
        };

        _dbContext.Campeonatos.Add(campeonato);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CampeonatoOperationResult.Success(CreateResponse(campeonato));
    }

    public async Task<CampeonatoOperationResult> UpdateCampeonatoAsync(
        int campeonatoId,
        UpdateCampeonatoRequest request,
        CancellationToken cancellationToken)
    {
        var campeonato = await _dbContext.Campeonatos
            .SingleOrDefaultAsync(campeonato => campeonato.IdCampeonato == campeonatoId, cancellationToken);

        if (campeonato is null)
        {
            return CampeonatoOperationResult.Failure("not_found", "Campeonato no encontrado.");
        }

        if (!string.Equals(campeonato.Estado, EstadoActivo, StringComparison.OrdinalIgnoreCase))
        {
            return CampeonatoOperationResult.Failure("inactive_championship", "Solo campeonatos activos pueden modificarse.");
        }

        campeonato.Nombre = request.Nombre.Trim();
        campeonato.FechaInicio = request.FechaInicio;
        campeonato.FechaFin = request.FechaFin;
        campeonato.FechaActualizacion = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CampeonatoOperationResult.Success(CreateResponse(campeonato));
    }

    public async Task<CampeonatoOperationResult> FinalizeCampeonatoAsync(int campeonatoId, CancellationToken cancellationToken)
    {
        var campeonato = await _dbContext.Campeonatos
            .SingleOrDefaultAsync(campeonato => campeonato.IdCampeonato == campeonatoId, cancellationToken);

        if (campeonato is null)
        {
            return CampeonatoOperationResult.Failure("not_found", "Campeonato no encontrado.");
        }

        if (string.Equals(campeonato.Estado, EstadoFinalizado, StringComparison.OrdinalIgnoreCase))
        {
            return CampeonatoOperationResult.Success(CreateResponse(campeonato));
        }

        if (!string.Equals(campeonato.Estado, EstadoActivo, StringComparison.OrdinalIgnoreCase))
        {
            return CampeonatoOperationResult.Failure("inactive_championship", "Solo campeonatos activos pueden finalizarse.");
        }

        campeonato.Estado = EstadoFinalizado;
        campeonato.FechaActualizacion = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CampeonatoOperationResult.Success(CreateResponse(campeonato));
    }

    private static CampeonatoResponse CreateResponse(Campeonato campeonato)
    {
        return new CampeonatoResponse(
            campeonato.IdCampeonato,
            campeonato.Nombre,
            campeonato.FechaInicio,
            campeonato.FechaFin,
            campeonato.Estado,
            campeonato.FechaCreacion,
            campeonato.FechaActualizacion);
    }
}
