using WallyBallBackend.Application.Posiciones;

namespace WallyBallBackend.Application.PortalJugador;

public sealed record PortalPosicionesCategoriaResponse(
    int IdCampeonatoCategoria,
    int IdCampeonato,
    string Campeonato,
    int IdCategoria,
    string Categoria,
    int IdEquipoJugador,
    string EquipoJugador,
    int? PosicionEquipo,
    IReadOnlyCollection<PosicionResponse> Posiciones);
