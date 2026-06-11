namespace WallyBallBackend.Domain.Entities;

public sealed class AuditoriaResultado
{
    public int IdAuditoriaResultado { get; set; }

    public int IdResultado { get; set; }

    public Resultado? Resultado { get; set; }

    public int IdPartido { get; set; }

    public int SetsLocalAnterior { get; set; }

    public int SetsVisitanteAnterior { get; set; }

    public int IdEquipoGanadorAnterior { get; set; }

    public int SetsLocalNuevo { get; set; }

    public int SetsVisitanteNuevo { get; set; }

    public int IdEquipoGanadorNuevo { get; set; }

    public string? Motivo { get; set; }

    public DateTime FechaCambio { get; set; } = DateTime.UtcNow;
}
