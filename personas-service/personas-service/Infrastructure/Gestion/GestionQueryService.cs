using Microsoft.EntityFrameworkCore;
using PersonasService.Application.Gestion;
using PersonasService.Domain.Entities;
using PersonasService.Infrastructure.Persistence.SqlServer;

namespace PersonasService.Infrastructure.Gestion;

public sealed class GestionQueryService : IGestionQueryService
{
    private readonly IdentityDbContext _dbContext;

    public GestionQueryService(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<GestionRoleResponse>> GetRolesAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Roles
            .AsNoTracking()
            .OrderBy(rol => rol.IdRol)
            .Select(rol => new GestionRoleResponse(
                rol.IdRol,
                rol.Nombre,
                rol.Descripcion,
                rol.Activo))
            .ToListAsync(cancellationToken);
    }

    public async Task<GestionRoleResponse?> GetRoleByIdAsync(int roleId, CancellationToken cancellationToken)
    {
        return await _dbContext.Roles
            .AsNoTracking()
            .Where(rol => rol.IdRol == roleId)
            .Select(rol => new GestionRoleResponse(
                rol.IdRol,
                rol.Nombre,
                rol.Descripcion,
                rol.Activo))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<GestionUserResponse>> GetUsersAsync(CancellationToken cancellationToken)
    {
        var users = await _dbContext.Usuarios
            .AsNoTracking()
            .Include(usuario => usuario.UsuarioRoles)
            .ThenInclude(usuarioRol => usuarioRol.Rol)
            .OrderBy(usuario => usuario.IdUsuario)
            .ToListAsync(cancellationToken);

        return users.Select(CreateUserResponse).ToList();
    }

    public async Task<GestionUserResponse?> GetUserByIdAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Usuarios
            .AsNoTracking()
            .Include(usuario => usuario.UsuarioRoles)
            .ThenInclude(usuarioRol => usuarioRol.Rol)
            .SingleOrDefaultAsync(usuario => usuario.IdUsuario == userId, cancellationToken);

        return user is null ? null : CreateUserResponse(user);
    }

    private static GestionUserResponse CreateUserResponse(Usuario user)
    {
        var roles = user.UsuarioRoles
            .Where(usuarioRol => usuarioRol.Rol is not null)
            .Select(usuarioRol => usuarioRol.Rol!.Nombre)
            .Distinct()
            .OrderBy(role => role)
            .ToArray();

        return new GestionUserResponse(
            user.IdUsuario,
            user.Email,
            user.NombreCompleto,
            user.Activo,
            user.AccessFailedCount,
            user.LockoutEndUtc,
            user.FechaCreacion,
            user.FechaActualizacion,
            user.PasswordChangedAtUtc,
            roles);
    }
}
