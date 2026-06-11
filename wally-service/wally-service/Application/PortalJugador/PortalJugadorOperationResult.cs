namespace WallyBallBackend.Application.PortalJugador;

public sealed class PortalJugadorOperationResult<T>
{
    private PortalJugadorOperationResult(bool succeeded, T? value, string? errorCode, string? errorMessage)
    {
        Succeeded = succeeded;
        Value = value;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public bool Succeeded { get; }

    public T? Value { get; }

    public string? ErrorCode { get; }

    public string? ErrorMessage { get; }

    public static PortalJugadorOperationResult<T> Success(T value)
    {
        return new PortalJugadorOperationResult<T>(true, value, null, null);
    }

    public static PortalJugadorOperationResult<T> Failure(string errorCode, string errorMessage)
    {
        return new PortalJugadorOperationResult<T>(false, default, errorCode, errorMessage);
    }
}
