using Microsoft.EntityFrameworkCore;
using WallyBallBackend.Application.Posiciones;
using WallyBallBackend.Infrastructure.Persistence.SqlServer;

namespace WallyBallBackend.Infrastructure.Posiciones;

public sealed class PosicionService : IPosicionService
{
    private readonly AppDbContext _dbContext;

    public PosicionService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<PosicionResponse>?> GetTablaPosicionesAsync(
        int campeonatoCategoriaId,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.CampeonatoCategorias
            .AsNoTracking()
            .AnyAsync(
                campeonatoCategoria => campeonatoCategoria.IdCampeonatoCategoria == campeonatoCategoriaId,
                cancellationToken);

        if (!exists)
        {
            return null;
        }

        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC SP_CrearTablaPosicionesCategoria {campeonatoCategoriaId}",
            cancellationToken);

        var posiciones = await _dbContext.TablaPosiciones
            .AsNoTracking()
            .Include(posicion => posicion.Equipo)
            .Where(posicion => posicion.IdCampeonatoCategoria == campeonatoCategoriaId)
            .OrderByDescending(posicion => posicion.Puntos)
            .ThenByDescending(posicion => posicion.Ganados)
            .ThenByDescending(posicion => posicion.SetsFavor - posicion.SetsContra)
            .ThenByDescending(posicion => posicion.PuntosFavor - posicion.PuntosContra)
            .ThenByDescending(posicion => posicion.SetsFavor)
            .ThenByDescending(posicion => posicion.PuntosFavor)
            .ThenBy(posicion => posicion.Equipo!.Nombre)
            .ToListAsync(cancellationToken);

        return posiciones
            .Select((posicion, index) => new PosicionResponse(
                index + 1,
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
                posicion.FechaActualizacion))
            .ToList();
    }
}
