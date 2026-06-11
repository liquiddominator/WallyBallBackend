namespace WallyBallBackend.Application.Fixture;

public sealed record FixtureResponse(
    int IdCampeonatoCategoria,
    int IdCampeonato,
    string CampeonatoNombre,
    int IdCategoria,
    string CategoriaNombre,
    IReadOnlyCollection<FaseFixtureResponse> Fases);

public sealed record FaseFixtureResponse(
    int IdFase,
    string Nombre,
    string Tipo,
    int Orden,
    string Estado,
    IReadOnlyCollection<JornadaFixtureResponse> Jornadas);

public sealed record JornadaFixtureResponse(
    int IdJornada,
    int NumeroJornada,
    DateOnly? FechaProgramada,
    string Estado,
    IReadOnlyCollection<PartidoResponse> Partidos);

public sealed record PartidoResponse(
    int IdPartido,
    int IdCampeonatoCategoria,
    int IdFase,
    string FaseNombre,
    int IdJornada,
    int NumeroJornada,
    DateOnly? FechaJornada,
    int IdEquipoLocal,
    string EquipoLocalNombre,
    int IdEquipoVisitante,
    string EquipoVisitanteNombre,
    DateTime? FechaHoraProgramada,
    string Estado,
    IReadOnlyCollection<ReprogramacionPartidoResponse> Reprogramaciones);

public sealed record ReprogramacionPartidoResponse(
    int IdReprogramacion,
    DateTime? FechaHoraAnterior,
    DateTime FechaHoraNueva,
    string? Motivo,
    DateTime FechaRegistro);
