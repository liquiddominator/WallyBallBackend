using System.Security.Claims;

namespace WallyBallBackend.Application.PortalJugador;

public interface IPortalJugadorService
{
    Task<PortalJugadorOperationResult<IReadOnlyCollection<PortalFixturePartidoResponse>>> GetFixturePersonalAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken);

    Task<PortalJugadorOperationResult<IReadOnlyCollection<PortalResultadosCategoriaResponse>>> GetResultadosAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken);

    Task<PortalJugadorOperationResult<IReadOnlyCollection<PortalPosicionesCategoriaResponse>>> GetPosicionesAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken);
}
