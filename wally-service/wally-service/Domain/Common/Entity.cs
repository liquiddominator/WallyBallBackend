namespace WallyBallBackend.Domain.Common;

public abstract class Entity
{
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }
}
