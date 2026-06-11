using WallyBallBackend.Domain.Common;

namespace WallyBallBackend.Domain.Entities;

public sealed class Equipo : Entity
{
    public int IdEquipo { get; set; }

    public int IdCampeonatoCategoria { get; set; }

    public CampeonatoCategoria? CampeonatoCategoria { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public ICollection<InscripcionEquipoJugador> Inscripciones { get; set; } = [];

    public ICollection<Partido> PartidosComoLocal { get; set; } = [];

    public ICollection<Partido> PartidosComoVisitante { get; set; } = [];

    public ICollection<Resultado> ResultadosGanados { get; set; } = [];

    public ICollection<TablaPosicion> TablaPosiciones { get; set; } = [];
}
