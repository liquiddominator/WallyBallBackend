namespace WallyBallBackend.Application.Equipos;

public sealed record EquipoOperationResult(
    bool Succeeded,
    EquipoResponse? Value,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static EquipoOperationResult Success(EquipoResponse value)
    {
        return new EquipoOperationResult(true, value, null, null);
    }

    public static EquipoOperationResult Failure(string errorCode, string errorMessage)
    {
        return new EquipoOperationResult(false, null, errorCode, errorMessage);
    }
}
