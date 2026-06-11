namespace WallyBallBackend.Domain.Entities;

public sealed class ReprogramacionPartido
{
    public int IdReprogramacion { get; set; }

    public int IdPartido { get; set; }

    public Partido? Partido { get; set; }

    public DateTime? FechaHoraAnterior { get; set; }

    public DateTime FechaHoraNueva { get; set; }

    public string? Motivo { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
