namespace WallyBallBackend.Application.Campeonatos;

public sealed record CreateCampeonatoRequest(
    string Nombre,
    DateOnly FechaInicio,
    DateOnly FechaFin);
