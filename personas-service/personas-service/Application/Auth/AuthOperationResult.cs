namespace PersonasService.Application.Auth;

public sealed record AuthOperationResult(
    bool Succeeded,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static AuthOperationResult Success()
    {
        return new AuthOperationResult(true, null, null);
    }

    public static AuthOperationResult Failure(string errorCode, string errorMessage)
    {
        return new AuthOperationResult(false, errorCode, errorMessage);
    }
}
