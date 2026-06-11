using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WallyBallBackend.Application.Posiciones;

namespace WallyBallBackend.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize(Roles = "ORGANIZADOR,JUGADOR")]
[Tags("Posiciones")]
public sealed class PosicionesController : ControllerBase
{
    private readonly IPosicionService _posicionService;

    public PosicionesController(IPosicionService posicionService)
    {
        _posicionService = posicionService;
    }

    [HttpGet("api/v{version:apiVersion}/campeonatos-categorias/{campeonatoCategoriaId:int}/posiciones")]
    [ProducesResponseType(typeof(IReadOnlyCollection<PosicionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTablaPosiciones(
        int campeonatoCategoriaId,
        CancellationToken cancellationToken)
    {
        var posiciones = await _posicionService.GetTablaPosicionesAsync(campeonatoCategoriaId, cancellationToken);

        return posiciones is null ? NotFound() : Ok(posiciones);
    }
}

