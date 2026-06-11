namespace WallyBallBackend.Application.PortalJugador;

public sealed record PortalFixturePartidoResponse(
    int IdPartido,
    int IdCampeonatoCategoria,
    int IdCampeonato,
    string Campeonato,
    int IdCategoria,
    string Categoria,
    int IdFase,
    string Fase,
    int IdJornada,
    int NumeroJornada,
    DateOnly? FechaJornada,
    int IdEquipoJugador,
    string EquipoJugador,
    int IdEquipoLocal,
    string EquipoLocal,
    int IdEquipoVisitante,
    string EquipoVisitante,
    DateTime? FechaHoraProgramada,
    string Estado);
