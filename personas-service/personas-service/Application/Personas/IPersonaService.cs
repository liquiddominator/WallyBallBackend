namespace PersonasService.Application.Personas;

public interface IPersonaService
{
    Task<PersonaOperationResult> CreateJugadorAsync(
        CreateJugadorPersonaRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PersonaResponse>> GetPersonasAsync(
        IReadOnlyCollection<int> ids,
        string? termino,
        string? cedula,
        CancellationToken cancellationToken);
}
