using Asp.Versioning;
using FluentValidation;
using PersonasService.Application.Personas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PersonasService.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize(Roles = "ORGANIZADOR")]
[Tags("Personas")]
[Route("api/v{version:apiVersion}/personas")]
public sealed class PersonasController : ControllerBase
{
    private readonly IPersonaService _personaService;
    private readonly IValidator<CreateJugadorPersonaRequest> _createJugadorValidator;

    public PersonasController(
        IPersonaService personaService,
        IValidator<CreateJugadorPersonaRequest> createJugadorValidator)
    {
        _personaService = personaService;
        _createJugadorValidator = createJugadorValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<PersonaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPersonas(
        [FromQuery] int[]? ids,
        [FromQuery] string? termino,
        [FromQuery] string? cedula,
        CancellationToken cancellationToken)
    {
        var personas = await _personaService.GetPersonasAsync(ids ?? [], termino, cedula, cancellationToken);

        return Ok(personas);
    }

    [HttpPost("jugadores")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ProducesResponseType(typeof(JugadorPersonaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateJugador(
        CreateJugadorPersonaRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createJugadorValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(CreateValidationError(validationResult.ToDictionary()));
        }

        var result = await _personaService.CreateJugadorAsync(request, cancellationToken);

        if (!result.Succeeded)
        {
            return result.ErrorCode is "duplicate_person" or "duplicate_email"
                ? Conflict(CreateError(result))
                : BadRequest(CreateError(result));
        }

        return Created($"/api/v1/personas/{result.Value!.IdPersona}", result.Value);
    }

    private static object CreateError(PersonaOperationResult result)
    {
        return new
        {
            code = result.ErrorCode,
            message = result.ErrorMessage
        };
    }

    private static object CreateValidationError(IDictionary<string, string[]> errors)
    {
        return new
        {
            code = "validation_error",
            message = "La solicitud contiene datos invalidos.",
            errors
        };
    }
}
