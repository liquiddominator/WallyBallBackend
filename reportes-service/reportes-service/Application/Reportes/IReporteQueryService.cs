namespace ReportesService.Application.Reportes;

public interface IReporteQueryService
{
    Task<IReadOnlyCollection<ReporteEquiposCategoriaResponse>> GetEquiposAsync(
        int? campeonatoId,
        int? campeonatoCategoriaId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ReporteJugadoresCategoriaResponse>> GetJugadoresAsync(
        int? campeonatoId,
        int? campeonatoCategoriaId,
        int? equipoId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ReporteResultadoResponse>> GetResultadosAsync(
        int? campeonatoCategoriaId,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ReportePosicionesCategoriaResponse>> GetPosicionesAsync(
        int? campeonatoId,
        int? campeonatoCategoriaId,
        CancellationToken cancellationToken);
}
