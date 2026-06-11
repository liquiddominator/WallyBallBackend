namespace WallyBallBackend.Application.Jugadores;

public sealed record JugadorOperationResult(
    bool Succeeded,
    JugadorResponse? Jugador,
    JugadorEquipoResponse? Inscripcion,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static JugadorOperationResult Success(JugadorResponse jugador)
    {
        return new JugadorOperationResult(true, jugador, null, null, null);
    }

    public static JugadorOperationResult Success(JugadorEquipoResponse inscripcion)
    {
        return new JugadorOperationResult(true, null, inscripcion, null, null);
    }

    public static JugadorOperationResult Failure(string errorCode, string errorMessage)
    {
        return new JugadorOperationResult(false, null, null, errorCode, errorMessage);
    }
}
