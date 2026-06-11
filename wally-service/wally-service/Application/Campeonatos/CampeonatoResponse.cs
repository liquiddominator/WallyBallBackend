namespace WallyBallBackend.Application.Campeonatos;

public sealed record CampeonatoResponse(
    int IdCampeonato,
    string Nombre,
    DateOnly FechaInicio,
    DateOnly? FechaFin,
    string Estado,
    DateTime FechaCreacion,
    DateTime? FechaActualizacion);
