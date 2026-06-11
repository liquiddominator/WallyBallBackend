namespace WallyBallBackend.Application.Campeonatos;

public sealed record UpdateCampeonatoRequest(
    string Nombre,
    DateOnly FechaInicio,
    DateOnly FechaFin);
