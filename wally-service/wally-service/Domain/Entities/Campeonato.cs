using WallyBallBackend.Domain.Common;

namespace WallyBallBackend.Domain.Entities;

public sealed class Campeonato : Entity
{
    public int IdCampeonato { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public string Estado { get; set; } = "ACTIVO";

    public ICollection<CampeonatoCategoria> CampeonatoCategorias { get; set; } = [];
}
