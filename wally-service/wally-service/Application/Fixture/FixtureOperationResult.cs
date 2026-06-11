namespace WallyBallBackend.Application.Fixture;

public sealed record FixtureOperationResult(
    bool Succeeded,
    FixtureResponse? Fixture,
    PartidoResponse? Partido,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static FixtureOperationResult Success(FixtureResponse fixture)
    {
        return new FixtureOperationResult(true, fixture, null, null, null);
    }

    public static FixtureOperationResult Success(PartidoResponse partido)
    {
        return new FixtureOperationResult(true, null, partido, null, null);
    }

    public static FixtureOperationResult Failure(string errorCode, string errorMessage)
    {
        return new FixtureOperationResult(false, null, null, errorCode, errorMessage);
    }
}
