using WallyBallBackend.Application.Resultados;

namespace WallyBallBackend.Application.PortalJugador;

public sealed record PortalResultadosCategoriaResponse(
    int IdCampeonatoCategoria,
    int IdCampeonato,
    string Campeonato,
    int IdCategoria,
    string Categoria,
    int IdEquipoJugador,
    string EquipoJugador,
    IReadOnlyCollection<ResultadoResponse> Resultados);
