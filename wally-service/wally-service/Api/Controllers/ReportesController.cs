using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WallyBallBackend.Application.Reportes;

namespace WallyBallBackend.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize(Roles = "ORGANIZADOR")]
[Tags("Reportes")]
[Route("api/v{version:apiVersion}/reportes")]
public sealed class ReportesController : ControllerBase
{
    private readonly IReporteService _reporteService;

    public ReportesController(IReporteService reporteService)
    {
        _reporteService = reporteService;
    }

    [HttpGet("equipos")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ReporteEquiposCategoriaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetReporteEquipos(
        [FromQuery] int? campeonatoId,
        [FromQuery] int? campeonatoCategoriaId,
        CancellationToken cancellationToken)
    {
        var reporte = await _reporteService.GetReporteEquiposAsync(campeonatoId, campeonatoCategoriaId, cancellationToken);

        return Ok(reporte);
    }

    [HttpGet("jugadores")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ReporteJugadoresCategoriaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetReporteJugadores(
        [FromQuery] int? campeonatoId,
        [FromQuery] int? campeonatoCategoriaId,
        [FromQuery] int? equipoId,
        CancellationToken cancellationToken)
    {
        var reporte = await _reporteService.GetReporteJugadoresAsync(campeonatoId, campeonatoCategoriaId, equipoId, cancellationToken);

        return Ok(reporte);
    }

    [HttpGet("resultados")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ReporteResultadoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetReporteResultados(
        [FromQuery] int? campeonatoCategoriaId,
        [FromQuery] DateOnly? fechaDesde,
        [FromQuery] DateOnly? fechaHasta,
        CancellationToken cancellationToken)
    {
        var reporte = await _reporteService.GetReporteResultadosAsync(
            campeonatoCategoriaId,
            fechaDesde,
            fechaHasta,
            cancellationToken);

        return Ok(reporte);
    }

    [HttpGet("posiciones")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ReportePosicionesCategoriaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetReportePosiciones(
        [FromQuery] int? campeonatoId,
        [FromQuery] int? campeonatoCategoriaId,
        CancellationToken cancellationToken)
    {
        var reporte = await _reporteService.GetReportePosicionesAsync(campeonatoId, campeonatoCategoriaId, cancellationToken);

        return Ok(reporte);
    }
}
