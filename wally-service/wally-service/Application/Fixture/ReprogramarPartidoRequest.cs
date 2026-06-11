namespace WallyBallBackend.Application.Fixture;

public sealed record ReprogramarPartidoRequest(
    DateTime FechaHoraNueva,
    string? Motivo);
