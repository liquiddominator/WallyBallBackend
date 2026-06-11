using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WallyBallBackend.Application.PortalJugador;

namespace WallyBallBackend.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize(Roles = "JUGADOR")]
[Tags("Portal Jugador")]
[Route("api/v{version:apiVersion}/portal/jugador")]
public sealed class PortalJugadorController : ControllerBase
{
    private readonly IPortalJugadorService _portalJugadorService;

    public PortalJugadorController(IPortalJugadorService portalJugadorService)
    {
        _portalJugadorService = portalJugadorService;
    }

    [HttpGet("fixture")]
    [ProducesResponseType(typeof(IReadOnlyCollection<PortalFixturePartidoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFixturePersonal(CancellationToken cancellationToken)
    {
        var result = await _portalJugadorService.GetFixturePersonalAsync(User, cancellationToken);

        return result.Succeeded ? Ok(result.Value) : CreateErrorResponse(result.ErrorCode!, result.ErrorMessage!);
    }

    [HttpGet("resultados")]
    [ProducesResponseType(typeof(IReadOnlyCollection<PortalResultadosCategoriaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResultados(CancellationToken cancellationToken)
    {
        var result = await _portalJugadorService.GetResultadosAsync(User, cancellationToken);

        return result.Succeeded ? Ok(result.Value) : CreateErrorResponse(result.ErrorCode!, result.ErrorMessage!);
    }

    [HttpGet("posiciones")]
    [ProducesResponseType(typeof(IReadOnlyCollection<PortalPosicionesCategoriaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPosiciones(CancellationToken cancellationToken)
    {
        var result = await _portalJugadorService.GetPosicionesAsync(User, cancellationToken);

        return result.Succeeded ? Ok(result.Value) : CreateErrorResponse(result.ErrorCode!, result.ErrorMessage!);
    }

    private IActionResult CreateErrorResponse(string code, string message)
    {
        var body = new
        {
            code,
            message
        };

        return code switch
        {
            "player_not_found" => NotFound(body),
            _ => Forbid()
        };
    }
}
