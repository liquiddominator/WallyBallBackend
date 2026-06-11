namespace WallyBallBackend.Application.Campeonatos;

public sealed record CampeonatoOperationResult(
    bool Succeeded,
    CampeonatoResponse? Value,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static CampeonatoOperationResult Success(CampeonatoResponse value)
    {
        return new CampeonatoOperationResult(true, value, null, null);
    }

    public static CampeonatoOperationResult Failure(string errorCode, string errorMessage)
    {
        return new CampeonatoOperationResult(false, null, errorCode, errorMessage);
    }
}
