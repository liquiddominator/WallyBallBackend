namespace PersonasService.Application.Auth;

public sealed record AuthResult(
    bool Succeeded,
    AuthResponse? Value,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static AuthResult Success(AuthResponse value)
    {
        return new AuthResult(true, value, null, null);
    }

    public static AuthResult Failure(string errorCode, string errorMessage)
    {
        return new AuthResult(false, null, errorCode, errorMessage);
    }
}
