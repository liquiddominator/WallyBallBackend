namespace WallyBallBackend.Domain.Entities;

public sealed class TablaPosicion
{
    public int IdPosicion { get; set; }

    public int IdCampeonatoCategoria { get; set; }

    public CampeonatoCategoria? CampeonatoCategoria { get; set; }

    public int IdEquipo { get; set; }

    public Equipo? Equipo { get; set; }

    public int PartidosJugados { get; set; }

    public int Ganados { get; set; }

    public int Perdidos { get; set; }

    public int SetsFavor { get; set; }

    public int SetsContra { get; set; }

    public int PuntosFavor { get; set; }

    public int PuntosContra { get; set; }

    public int Puntos { get; set; }

    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
}
