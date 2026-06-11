namespace ReportesService.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "PersonasService";

    public string Audience { get; set; } = "WallyBallClients";

    public string SigningKey { get; set; } = "development-signing-key-change-before-production-32";
}
