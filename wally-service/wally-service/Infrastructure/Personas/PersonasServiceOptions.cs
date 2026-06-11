namespace WallyBallBackend.Infrastructure.Personas;

public sealed class PersonasServiceOptions
{
    public const string SectionName = "PersonasService";

    public string BaseUrl { get; set; } = "http://localhost:5097";
}
