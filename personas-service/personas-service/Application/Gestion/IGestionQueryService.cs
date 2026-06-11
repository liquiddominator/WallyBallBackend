namespace PersonasService.Application.Gestion;

public interface IGestionQueryService
{
    Task<IReadOnlyCollection<GestionRoleResponse>> GetRolesAsync(CancellationToken cancellationToken);

    Task<GestionRoleResponse?> GetRoleByIdAsync(int roleId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<GestionUserResponse>> GetUsersAsync(CancellationToken cancellationToken);

    Task<GestionUserResponse?> GetUserByIdAsync(int userId, CancellationToken cancellationToken);
}
