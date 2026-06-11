namespace WallyBallBackend.Domain.Entities;

public sealed class Jornada
{
    public int IdJornada { get; set; }

    public int IdFase { get; set; }

    public Fase? Fase { get; set; }

    public int NumeroJornada { get; set; }

    public DateOnly? FechaProgramada { get; set; }

    public string Estado { get; set; } = "PROGRAMADA";

    public ICollection<Partido> Partidos { get; set; } = [];
}
