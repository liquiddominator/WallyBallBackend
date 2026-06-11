namespace WallyBallBackend.Domain.Entities;

public sealed class Fase
{
    public int IdFase { get; set; }

    public int IdCampeonatoCategoria { get; set; }

    public CampeonatoCategoria? CampeonatoCategoria { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Tipo { get; set; } = "ROUND_ROBIN";

    public int Orden { get; set; }

    public string Estado { get; set; } = "PENDIENTE";

    public ICollection<Jornada> Jornadas { get; set; } = [];

    public ICollection<Partido> Partidos { get; set; } = [];
}
