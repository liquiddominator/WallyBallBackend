using WallyBallBackend.Domain.Common;

namespace WallyBallBackend.Domain.Entities;

public sealed class CampeonatoCategoria : Entity
{
    public int IdCampeonatoCategoria { get; set; }

    public int IdCampeonato { get; set; }

    public Campeonato? Campeonato { get; set; }

    public int IdCategoria { get; set; }

    public Categoria? Categoria { get; set; }

    public string Estado { get; set; } = "ACTIVA";

    public ICollection<Equipo> Equipos { get; set; } = [];

    public ICollection<Fase> Fases { get; set; } = [];

    public ICollection<Partido> Partidos { get; set; } = [];

    public ICollection<TablaPosicion> TablaPosiciones { get; set; } = [];
}
