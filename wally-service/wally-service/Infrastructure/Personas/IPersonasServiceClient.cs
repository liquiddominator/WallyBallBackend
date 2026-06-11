namespace WallyBallBackend.Infrastructure.Personas;

public interface IPersonasServiceClient
{
    Task<PersonasClientResult<JugadorPersonaClientResponse>> CreateJugadorAsync(
        CreateJugadorPersonaRequest request,
        CancellationToken cancellationToken);

    Task<PersonasClientResult<IReadOnlyCollection<PersonaClientResponse>>> GetPersonasAsync(
        IReadOnlyCollection<int> ids,
        string? termino,
        string? cedula,
        CancellationToken cancellationToken);
}
