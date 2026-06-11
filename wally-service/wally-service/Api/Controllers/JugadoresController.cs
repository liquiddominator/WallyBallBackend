using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WallyBallBackend.Application.Jugadores;

namespace WallyBallBackend.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize(Roles = "ORGANIZADOR")]
[Tags("Jugadores")]
[Route("api/v{version:apiVersion}/jugadores")]
public sealed class JugadoresController : ControllerBase
{
    private readonly IJugadorService _jugadorService;
    private readonly IValidator<CreateJugadorRequest> _createValidator;
    private readonly IValidator<AsignarJugadorEquipoRequest> _asignarValidator;

    public JugadoresController(
        IJugadorService jugadorService,
        IValidator<CreateJugadorRequest> createValidator,
        IValidator<AsignarJugadorEquipoRequest> asignarValidator)
    {
        _jugadorService = jugadorService;
        _createValidator = createValidator;
        _asignarValidator = asignarValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<JugadorResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetJugadores(
        [FromQuery] string? termino,
        [FromQuery] string? cedula,
        [FromQuery] int? equipoId,
        CancellationToken cancellationToken)
    {
        var jugadores = await _jugadorService.GetJugadoresAsync(termino, cedula, equipoId, cancellationToken);

        return Ok(jugadores);
    }

    [HttpGet("{jugadorId:int}")]
    [ProducesResponseType(typeof(JugadorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJugadorById(int jugadorId, CancellationToken cancellationToken)
    {
        var jugador = await _jugadorService.GetJugadorByIdAsync(jugadorId, cancellationToken);

        return jugador is null ? NotFound() : Ok(jugador);
    }

    [HttpPost]
    [ProducesResponseType(typeof(JugadorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateJugador(
        CreateJugadorRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(CreateValidationError(validationResult.ToDictionary()));
        }

        var result = await _jugadorService.CreateJugadorAsync(request, cancellationToken);

        if (!result.Succeeded)
        {
            return CreateErrorResponse(result);
        }

        var location = $"/api/v1/jugadores/{result.Jugador!.IdJugador}";

        return Created(location, result.Jugador);
    }

    [HttpPost("~/api/v{version:apiVersion}/equipos/{equipoId:int}/jugadores")]
    [ProducesResponseType(typeof(JugadorEquipoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AsignarJugadorEquipo(
        int equipoId,
        AsignarJugadorEquipoRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _asignarValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(CreateValidationError(validationResult.ToDictionary()));
        }

        var result = await _jugadorService.AsignarJugadorEquipoAsync(equipoId, request, cancellationToken);

        if (!result.Succeeded)
        {
            return CreateErrorResponse(result);
        }

        var location = $"/api/v1/equipos/{equipoId}/jugadores";

        return Created(location, result.Inscripcion);
    }

    [HttpGet("~/api/v{version:apiVersion}/equipos/{equipoId:int}/jugadores")]
    [ProducesResponseType(typeof(IReadOnlyCollection<JugadorEquipoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetJugadoresByEquipo(int equipoId, CancellationToken cancellationToken)
    {
        var jugadores = await _jugadorService.GetJugadoresByEquipoAsync(equipoId, cancellationToken);

        return Ok(jugadores);
    }

    private IActionResult CreateErrorResponse(JugadorOperationResult result)
    {
        return result.ErrorCode switch
        {
            "team_not_found" => NotFound(CreateError(result)),
            "player_not_found" => NotFound(CreateError(result)),
            "duplicate_player" => Conflict(CreateError(result)),
            "duplicate_assignment" => Conflict(CreateError(result)),
            "team_full" => Conflict(CreateError(result)),
            "inactive_team" => Conflict(CreateError(result)),
            "inactive_player" => Conflict(CreateError(result)),
            "inactive_championship" => Conflict(CreateError(result)),
            _ => BadRequest(CreateError(result))
        };
    }

    private static object CreateError(JugadorOperationResult result)
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
