namespace WallyBallBackend.Domain.Entities;

public sealed class InscripcionEquipoJugador
{
    public int IdInscripcion { get; set; }

    public int IdEquipo { get; set; }

    public Equipo? Equipo { get; set; }

    public int IdJugador { get; set; }

    public Jugador? Jugador { get; set; }

    public DateTime FechaInscripcion { get; set; } = DateTime.UtcNow;

    public string Estado { get; set; } = "ACTIVO";
}
