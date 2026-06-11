namespace WallyBallBackend.Infrastructure.Personas;

public sealed class PersonasClientResult<T>
{
    private PersonasClientResult(
        bool succeeded,
        T? value,
        string? errorCode,
        string? errorMessage)
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

    public static PersonasClientResult<T> Success(T value)
    {
        return new PersonasClientResult<T>(true, value, null, null);
    }

    public static PersonasClientResult<T> Failure(string errorCode, string errorMessage)
    {
        return new PersonasClientResult<T>(false, default, errorCode, errorMessage);
    }
}
