namespace PersonasService.Application.Personas;

public sealed class PersonaOperationResult
{
    private PersonaOperationResult(
        bool succeeded,
        JugadorPersonaResponse? value,
        string? errorCode,
        string? errorMessage)
    {
        Succeeded = succeeded;
        Value = value;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public bool Succeeded { get; }

    public JugadorPersonaResponse? Value { get; }

    public string? ErrorCode { get; }

    public string? ErrorMessage { get; }

    public static PersonaOperationResult Success(JugadorPersonaResponse value)
    {
        return new PersonaOperationResult(true, value, null, null);
    }

    public static PersonaOperationResult Failure(string errorCode, string errorMessage)
    {
        return new PersonaOperationResult(false, null, errorCode, errorMessage);
    }
}
