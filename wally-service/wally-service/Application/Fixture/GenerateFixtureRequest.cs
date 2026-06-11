namespace WallyBallBackend.Application.Fixture;

public sealed record GenerateFixtureRequest(
    DateOnly? FechaPrimeraJornada,
    int DiasEntreJornadas,
    TimeOnly? HoraPartidos);
