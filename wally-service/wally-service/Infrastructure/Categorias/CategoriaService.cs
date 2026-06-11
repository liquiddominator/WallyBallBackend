using Microsoft.EntityFrameworkCore;
using WallyBallBackend.Application.Categorias;
using WallyBallBackend.Domain.Entities;
using WallyBallBackend.Infrastructure.Persistence.SqlServer;

namespace WallyBallBackend.Infrastructure.Categorias;

public sealed class CategoriaService : ICategoriaService
{
    private const string EstadoActivo = "ACTIVO";
    private const string EstadoCategoriaActiva = "ACTIVA";

    private readonly AppDbContext _dbContext;

    public CategoriaService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<CategoriaResponse>> GetCategoriasAsync(
        int? campeonatoId,
        CancellationToken cancellationToken)
    {
        if (campeonatoId.HasValue)
        {
            return await _dbContext.CampeonatoCategorias
                .AsNoTracking()
                .Where(campeonatoCategoria => campeonatoCategoria.IdCampeonato == campeonatoId.Value)
                .OrderBy(campeonatoCategoria => campeonatoCategoria.Categoria!.Nombre)
                .ThenBy(campeonatoCategoria => campeonatoCategoria.IdCampeonatoCategoria)
                .Select(campeonatoCategoria => new CategoriaResponse(
                    campeonatoCategoria.IdCategoria,
                    campeonatoCategoria.IdCampeonatoCategoria,
                    campeonatoCategoria.IdCampeonato,
                    campeonatoCategoria.Campeonato!.Nombre,
                    campeonatoCategoria.Categoria!.Nombre,
                    campeonatoCategoria.Estado,
                    campeonatoCategoria.FechaCreacion,
                    campeonatoCategoria.FechaActualizacion))
                .ToListAsync(cancellationToken);
        }

        return await _dbContext.Categorias
            .AsNoTracking()
            .OrderBy(categoria => categoria.Nombre)
            .ThenBy(categoria => categoria.IdCategoria)
            .Select(categoria => CreateResponse(categoria))
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoriaResponse?> GetCategoriaByIdAsync(int categoriaId, CancellationToken cancellationToken)
    {
        return await _dbContext.Categorias
            .AsNoTracking()
            .Where(categoria => categoria.IdCategoria == categoriaId)
            .Select(categoria => CreateResponse(categoria))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<CategoriaOperationResult> CreateCategoriaAsync(CreateCategoriaRequest request, CancellationToken cancellationToken)
    {
        var nombre = request.Nombre.Trim();

        var exists = await _dbContext.Categorias
            .AnyAsync(categoria => categoria.Nombre == nombre, cancellationToken);

        if (exists)
        {
            return CategoriaOperationResult.Failure("duplicate_category", "Ya existe una categoria con ese nombre.");
        }

        var categoria = new Categoria
        {
            Nombre = nombre,
            Estado = EstadoCategoriaActiva
        };

        _dbContext.Categorias.Add(categoria);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicateCategoryException(exception))
        {
            return CategoriaOperationResult.Failure("duplicate_category", "Ya existe una categoria con ese nombre.");
        }
        catch (DbUpdateException)
        {
            return CategoriaOperationResult.Failure("category_persistence_error", "No se pudo registrar la categoria en la base de datos.");
        }

        return CategoriaOperationResult.Success(CreateResponse(categoria));
    }

    public async Task<CategoriaOperationResult> AddCategoriaToCampeonatoAsync(
        int campeonatoId,
        AddCategoriaCampeonatoRequest request,
        CancellationToken cancellationToken)
    {
        var campeonato = await _dbContext.Campeonatos
            .SingleOrDefaultAsync(campeonato => campeonato.IdCampeonato == campeonatoId, cancellationToken);

        if (campeonato is null)
        {
            return CategoriaOperationResult.Failure("championship_not_found", "Campeonato no encontrado.");
        }

        if (!string.Equals(campeonato.Estado, EstadoActivo, StringComparison.OrdinalIgnoreCase))
        {
            return CategoriaOperationResult.Failure("inactive_championship", "No se pueden asociar categorias a un campeonato finalizado o inactivo.");
        }

        var categoria = await _dbContext.Categorias
            .SingleOrDefaultAsync(categoria => categoria.IdCategoria == request.IdCategoria, cancellationToken);

        if (categoria is null)
        {
            return CategoriaOperationResult.Failure("category_not_found", "Categoria no encontrada.");
        }

        if (!string.Equals(categoria.Estado, EstadoCategoriaActiva, StringComparison.OrdinalIgnoreCase))
        {
            return CategoriaOperationResult.Failure("inactive_category", "No se puede asociar una categoria inactiva.");
        }

        var exists = await _dbContext.CampeonatoCategorias
            .AnyAsync(
                campeonatoCategoria => campeonatoCategoria.IdCampeonato == campeonatoId
                    && campeonatoCategoria.IdCategoria == request.IdCategoria,
                cancellationToken);

        if (exists)
        {
            return CategoriaOperationResult.Failure("duplicate_championship_category", "La categoria ya esta asociada a ese campeonato.");
        }

        var campeonatoCategoria = new CampeonatoCategoria
        {
            IdCampeonato = campeonatoId,
            Campeonato = campeonato,
            IdCategoria = categoria.IdCategoria,
            Categoria = categoria,
            Estado = EstadoCategoriaActiva
        };

        _dbContext.CampeonatoCategorias.Add(campeonatoCategoria);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicateChampionshipCategoryException(exception))
        {
            return CategoriaOperationResult.Failure("duplicate_championship_category", "La categoria ya esta asociada a ese campeonato.");
        }
        catch (DbUpdateException)
        {
            return CategoriaOperationResult.Failure("category_persistence_error", "No se pudo asociar la categoria al campeonato.");
        }

        return CategoriaOperationResult.Success(CreateResponse(campeonatoCategoria));
    }

    private static bool IsDuplicateCategoryException(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;

        return message.Contains("UQ_Categorias_Nombre", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Cannot insert duplicate key", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Violation of UNIQUE KEY constraint", StringComparison.OrdinalIgnoreCase);
    }

    private static CategoriaResponse CreateResponse(Categoria categoria)
    {
        return new CategoriaResponse(
            categoria.IdCategoria,
            IdCampeonatoCategoria: null,
            IdCampeonato: null,
            CampeonatoNombre: null,
            categoria.Nombre,
            categoria.Estado,
            categoria.FechaCreacion,
            categoria.FechaActualizacion);
    }

    private static bool IsDuplicateChampionshipCategoryException(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;

        return message.Contains("UQ_CampeonatosCategorias_Campeonato_Categoria", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Cannot insert duplicate key", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Violation of UNIQUE KEY constraint", StringComparison.OrdinalIgnoreCase);
    }

    private static CategoriaResponse CreateResponse(CampeonatoCategoria campeonatoCategoria)
    {
        return new CategoriaResponse(
            campeonatoCategoria.IdCategoria,
            campeonatoCategoria.IdCampeonatoCategoria,
            campeonatoCategoria.IdCampeonato,
            campeonatoCategoria.Campeonato?.Nombre,
            campeonatoCategoria.Categoria?.Nombre ?? string.Empty,
            campeonatoCategoria.Estado,
            campeonatoCategoria.FechaCreacion,
            campeonatoCategoria.FechaActualizacion);
    }
}
