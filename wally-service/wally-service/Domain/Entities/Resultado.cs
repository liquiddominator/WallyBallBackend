namespace WallyBallBackend.Domain.Entities;

public sealed class Resultado
{
    public int IdResultado { get; set; }

    public int IdPartido { get; set; }

    public Partido? Partido { get; set; }

    public int SetsLocal { get; set; }

    public int SetsVisitante { get; set; }

    public int IdEquipoGanador { get; set; }

    public Equipo? EquipoGanador { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<ResultadoSet> Sets { get; set; } = [];

    public ICollection<AuditoriaResultado> Auditorias { get; set; } = [];
}
