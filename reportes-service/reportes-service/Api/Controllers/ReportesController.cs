using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportesService.Application.Reportes;

namespace ReportesService.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize(Roles = "ORGANIZADOR")]
[Tags("Reportes")]
[Route("api/v{version:apiVersion}/reportes")]
public sealed class ReportesController : ControllerBase
{
    private readonly IReporteQueryService _reporteQueryService;

    public ReportesController(IReporteQueryService reporteQueryService)
    {
        _reporteQueryService = reporteQueryService;
    }

    [HttpGet("equipos")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ReporteEquiposCategoriaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEquipos(
        [FromQuery] int? campeonatoId,
        [FromQuery] int? campeonatoCategoriaId,
        CancellationToken cancellationToken)
    {
        return Ok(await _reporteQueryService.GetEquiposAsync(campeonatoId, campeonatoCategoriaId, cancellationToken));
    }

    [HttpGet("jugadores")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ReporteJugadoresCategoriaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetJugadores(
        [FromQuery] int? campeonatoId,
        [FromQuery] int? campeonatoCategoriaId,
        [FromQuery] int? equipoId,
        CancellationToken cancellationToken)
    {
        return Ok(await _reporteQueryService.GetJugadoresAsync(campeonatoId, campeonatoCategoriaId, equipoId, cancellationToken));
    }

    [HttpGet("resultados")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ReporteResultadoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetResultados(
        [FromQuery] int? campeonatoCategoriaId,
        [FromQuery] DateOnly? fechaDesde,
        [FromQuery] DateOnly? fechaHasta,
        CancellationToken cancellationToken)
    {
        return Ok(await _reporteQueryService.GetResultadosAsync(campeonatoCategoriaId, fechaDesde, fechaHasta, cancellationToken));
    }

    [HttpGet("posiciones")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ReportePosicionesCategoriaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPosiciones(
        [FromQuery] int? campeonatoId,
        [FromQuery] int? campeonatoCategoriaId,
        CancellationToken cancellationToken)
    {
        return Ok(await _reporteQueryService.GetPosicionesAsync(campeonatoId, campeonatoCategoriaId, cancellationToken));
    }
}
