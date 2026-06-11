namespace WallyBallBackend.Application.DatosPrueba;

public sealed class DatosPruebaOperationResult
{
    private DatosPruebaOperationResult(
        bool succeeded,
        DatosPruebaResponse? value,
        string? errorCode,
        string? errorMessage)
    {
        Succeeded = succeeded;
        Value = value;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public bool Succeeded { get; }

    public DatosPruebaResponse? Value { get; }

    public string? ErrorCode { get; }

    public string? ErrorMessage { get; }

    public static DatosPruebaOperationResult Success(DatosPruebaResponse value)
    {
        return new DatosPruebaOperationResult(true, value, null, null);
    }

    public static DatosPruebaOperationResult Failure(string errorCode, string errorMessage)
    {
        return new DatosPruebaOperationResult(false, null, errorCode, errorMessage);
    }
}

