namespace WallyBallBackend.Application.Reportes;

public interface IReporteService
{
    Task<IReadOnlyCollection<ReporteEquiposCategoriaResponse>> GetReporteEquiposAsync(
        int? campeonatoId,
        int? campeonatoCategoriaId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ReporteJugadoresCategoriaResponse>> GetReporteJugadoresAsync(
        int? campeonatoId,
        int? campeonatoCategoriaId,
        int? equipoId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ReporteResultadoResponse>> GetReporteResultadosAsync(
        int? campeonatoCategoriaId,
        DateOnly? fechaDesde,
        DateOnly? fechaHasta,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ReportePosicionesCategoriaResponse>> GetReportePosicionesAsync(
        int? campeonatoId,
        int? campeonatoCategoriaId,
        CancellationToken cancellationToken);
}
