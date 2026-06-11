namespace WallyBallBackend.Application.Categorias;

public sealed record CategoriaOperationResult(
    bool Succeeded,
    CategoriaResponse? Value,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static CategoriaOperationResult Success(CategoriaResponse value)
    {
        return new CategoriaOperationResult(true, value, null, null);
    }

    public static CategoriaOperationResult Failure(string errorCode, string errorMessage)
    {
        return new CategoriaOperationResult(false, null, errorCode, errorMessage);
    }
}
