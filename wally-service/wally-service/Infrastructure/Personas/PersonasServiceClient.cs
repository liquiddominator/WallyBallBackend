using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace WallyBallBackend.Infrastructure.Personas;

public sealed class PersonasServiceClient : IPersonasServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PersonasServiceClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PersonasClientResult<JugadorPersonaClientResponse>> CreateJugadorAsync(
        CreateJugadorPersonaRequest request,
        CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/v1/personas/jugadores")
        {
            Content = JsonContent.Create(request)
        };

        AddAuthorization(httpRequest);

        try
        {
            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var value = await response.Content.ReadFromJsonAsync<JugadorPersonaClientResponse>(cancellationToken);

                return value is null
                    ? PersonasClientResult<JugadorPersonaClientResponse>.Failure("personas_invalid_response", "personas-service devolvio una respuesta invalida.")
                    : PersonasClientResult<JugadorPersonaClientResponse>.Success(value);
            }

            var error = await ReadErrorAsync(response, cancellationToken);

            return PersonasClientResult<JugadorPersonaClientResponse>.Failure(
                error.Code ?? CreateErrorCode(response.StatusCode),
                error.Message ?? "No se pudo crear la persona del jugador.");
        }
        catch (HttpRequestException)
        {
            return PersonasClientResult<JugadorPersonaClientResponse>.Failure("personas_service_unavailable", "No se pudo conectar con personas-service.");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return PersonasClientResult<JugadorPersonaClientResponse>.Failure("personas_service_timeout", "personas-service no respondio a tiempo.");
        }
    }

    public async Task<PersonasClientResult<IReadOnlyCollection<PersonaClientResponse>>> GetPersonasAsync(
        IReadOnlyCollection<int> ids,
        string? termino,
        string? cedula,
        CancellationToken cancellationToken)
    {
        var query = new List<string>();

        foreach (var id in ids.Distinct())
        {
            query.Add($"ids={id}");
        }

        if (!string.IsNullOrWhiteSpace(termino))
        {
            query.Add($"termino={Uri.EscapeDataString(termino)}");
        }

        if (!string.IsNullOrWhiteSpace(cedula))
        {
            query.Add($"cedula={Uri.EscapeDataString(cedula)}");
        }

        var uri = query.Count == 0
            ? "api/v1/personas"
            : $"api/v1/personas?{string.Join("&", query)}";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, uri);
        AddAuthorization(httpRequest);

        try
        {
            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var value = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<PersonaClientResponse>>(cancellationToken);

                return PersonasClientResult<IReadOnlyCollection<PersonaClientResponse>>.Success(value ?? []);
            }

            var error = await ReadErrorAsync(response, cancellationToken);

            return PersonasClientResult<IReadOnlyCollection<PersonaClientResponse>>.Failure(
                error.Code ?? CreateErrorCode(response.StatusCode),
                error.Message ?? "No se pudieron consultar personas.");
        }
        catch (HttpRequestException)
        {
            return PersonasClientResult<IReadOnlyCollection<PersonaClientResponse>>.Failure("personas_service_unavailable", "No se pudo conectar con personas-service.");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return PersonasClientResult<IReadOnlyCollection<PersonaClientResponse>>.Failure("personas_service_timeout", "personas-service no respondio a tiempo.");
        }
    }

    private void AddAuthorization(HttpRequestMessage request)
    {
        var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

        if (!string.IsNullOrWhiteSpace(authorizationHeader))
        {
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(authorizationHeader);
        }
    }

    private static async Task<ClientErrorResponse> ReadErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ClientErrorResponse>(cancellationToken)
                ?? new ClientErrorResponse(null, null);
        }
        catch (JsonException)
        {
            return new ClientErrorResponse(null, null);
        }
    }

    private static string CreateErrorCode(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.Unauthorized => "personas_unauthorized",
            HttpStatusCode.Forbidden => "personas_forbidden",
            HttpStatusCode.Conflict => "personas_conflict",
            _ => "personas_error"
        };
    }

    private sealed record ClientErrorResponse(string? Code, string? Message);
}
