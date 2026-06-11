namespace WallyBallBackend.Application.Posiciones;

public sealed record PosicionResponse(
    int Posicion,
    int IdPosicion,
    int IdCampeonatoCategoria,
    int IdEquipo,
    string Equipo,
    int PartidosJugados,
    int Ganados,
    int Perdidos,
    int SetsFavor,
    int SetsContra,
    int DiferenciaSets,
    int PuntosFavor,
    int PuntosContra,
    int DiferenciaPuntos,
    int Puntos,
    DateTime FechaActualizacion);
