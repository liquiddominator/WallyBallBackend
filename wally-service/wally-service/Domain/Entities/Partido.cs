namespace WallyBallBackend.Domain.Entities;

public sealed class Partido
{
    public int IdPartido { get; set; }

    public int IdCampeonatoCategoria { get; set; }

    public CampeonatoCategoria? CampeonatoCategoria { get; set; }

    public int IdFase { get; set; }

    public Fase? Fase { get; set; }

    public int IdJornada { get; set; }

    public Jornada? Jornada { get; set; }

    public int IdEquipoLocal { get; set; }

    public Equipo? EquipoLocal { get; set; }

    public int IdEquipoVisitante { get; set; }

    public Equipo? EquipoVisitante { get; set; }

    public DateTime? FechaHoraProgramada { get; set; }

    public string Estado { get; set; } = "PROGRAMADO";

    public Resultado? Resultado { get; set; }

    public ICollection<ReprogramacionPartido> Reprogramaciones { get; set; } = [];
}
