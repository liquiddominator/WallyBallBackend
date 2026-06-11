namespace WallyBallBackend.Domain.Entities;

public sealed class ResultadoSet
{
    public int IdResultadoSet { get; set; }

    public int IdResultado { get; set; }

    public Resultado? Resultado { get; set; }

    public int NumeroSet { get; set; }

    public int PuntosLocal { get; set; }

    public int PuntosVisitante { get; set; }
}
