using WallyBallBackend.Domain.Common;

namespace WallyBallBackend.Domain.Entities;

public sealed class Categoria : Entity
{
    public int IdCategoria { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Estado { get; set; } = "ACTIVA";

    public ICollection<CampeonatoCategoria> CampeonatoCategorias { get; set; } = [];
}
