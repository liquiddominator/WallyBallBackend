namespace WallyBallBackend.Application.Equipos;

public sealed record EquipoResponse(
    int IdEquipo,
    int IdCampeonatoCategoria,
    int IdCategoria,
    string CategoriaNombre,
    int IdCampeonato,
    string CampeonatoNombre,
    string Nombre,
    bool Activo,
    int CantidadJugadores,
    DateTime FechaCreacion,
    DateTime? FechaActualizacion);
