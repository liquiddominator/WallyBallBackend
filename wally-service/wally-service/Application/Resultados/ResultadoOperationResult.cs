namespace WallyBallBackend.Application.Resultados;

public sealed class ResultadoOperationResult
{
    private ResultadoOperationResult(bool succeeded, ResultadoResponse? resultado, string? errorCode, string? errorMessage)
    {
        Succeeded = succeeded;
        Resultado = resultado;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public bool Succeeded { get; }

    public ResultadoResponse? Resultado { get; }

    public string? ErrorCode { get; }

    public string? ErrorMessage { get; }

    public static ResultadoOperationResult Success(ResultadoResponse resultado)
    {
        return new ResultadoOperationResult(true, resultado, null, null);
    }

    public static ResultadoOperationResult Failure(string errorCode, string errorMessage)
    {
        return new ResultadoOperationResult(false, null, errorCode, errorMessage);
    }
}

