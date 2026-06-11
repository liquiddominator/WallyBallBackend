namespace IdentidadService.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "IdentidadService";

    public string Audience { get; set; } = "WallyBallClients";

    public string SigningKey { get; set; } = "development-signing-key-change-before-production-32";

    public int ExpirationMinutes { get; set; } = 120;

    public int RefreshTokenExpirationDays { get; set; } = 7;

    public int MaxFailedLoginAttempts { get; set; } = 5;

    public int LockoutMinutes { get; set; } = 15;
}
