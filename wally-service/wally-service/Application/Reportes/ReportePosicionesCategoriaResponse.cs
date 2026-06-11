using WallyBallBackend.Application.Posiciones;

namespace WallyBallBackend.Application.Reportes;

public sealed record ReportePosicionesCategoriaResponse(
    int IdCampeonatoCategoria,
    int IdCampeonato,
    string Campeonato,
    int IdCategoria,
    string Categoria,
    DateTime? FechaActualizacion,
    IReadOnlyCollection<PosicionResponse> Posiciones);
