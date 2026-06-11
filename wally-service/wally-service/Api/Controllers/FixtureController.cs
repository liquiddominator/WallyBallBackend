using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WallyBallBackend.Application.Fixture;

namespace WallyBallBackend.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize(Roles = "ORGANIZADOR")]
[Tags("Fixture")]
public sealed class FixtureController : ControllerBase
{
    private readonly IFixtureService _fixtureService;
    private readonly IValidator<GenerateFixtureRequest> _generateValidator;
    private readonly IValidator<ReprogramarPartidoRequest> _reprogramarValidator;

    public FixtureController(
        IFixtureService fixtureService,
        IValidator<GenerateFixtureRequest> generateValidator,
        IValidator<ReprogramarPartidoRequest> reprogramarValidator)
    {
        _fixtureService = fixtureService;
        _generateValidator = generateValidator;
        _reprogramarValidator = reprogramarValidator;
    }

    [HttpGet("api/v{version:apiVersion}/campeonatos-categorias/{campeonatoCategoriaId:int}/fixture")]
    [ProducesResponseType(typeof(FixtureResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFixture(int campeonatoCategoriaId, CancellationToken cancellationToken)
    {
        var fixture = await _fixtureService.GetFixtureAsync(campeonatoCategoriaId, cancellationToken);

        return fixture is null ? NotFound() : Ok(fixture);
    }

    [HttpPost("api/v{version:apiVersion}/campeonatos-categorias/{campeonatoCategoriaId:int}/fixture/generar")]
    [ProducesResponseType(typeof(FixtureResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GenerateFixture(
        int campeonatoCategoriaId,
        GenerateFixtureRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _generateValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(CreateValidationError(validationResult.ToDictionary()));
        }

        var result = await _fixtureService.GenerateFixtureAsync(campeonatoCategoriaId, request, cancellationToken);

        if (!result.Succeeded)
        {
            return CreateErrorResponse(result);
        }

        var location = $"/api/v1/campeonatos-categorias/{campeonatoCategoriaId}/fixture";

        return Created(location, result.Fixture);
    }

    [HttpPatch("api/v{version:apiVersion}/partidos/{partidoId:int}/reprogramar")]
    [ProducesResponseType(typeof(PartidoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReprogramarPartido(
        int partidoId,
        ReprogramarPartidoRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _reprogramarValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(CreateValidationError(validationResult.ToDictionary()));
        }

        var result = await _fixtureService.ReprogramarPartidoAsync(partidoId, request, cancellationToken);

        if (!result.Succeeded)
        {
            return CreateErrorResponse(result);
        }

        return Ok(result.Partido);
    }

    private IActionResult CreateErrorResponse(FixtureOperationResult result)
    {
        return result.ErrorCode switch
        {
            "championship_category_not_found" => NotFound(CreateError(result)),
            "match_not_found" => NotFound(CreateError(result)),
            "inactive_championship" => Conflict(CreateError(result)),
            "inactive_championship_category" => Conflict(CreateError(result)),
            "fixture_already_exists" => Conflict(CreateError(result)),
            "not_enough_teams" => Conflict(CreateError(result)),
            "match_not_reprogrammable" => Conflict(CreateError(result)),
            "invalid_fixture" => Conflict(CreateError(result)),
            _ => BadRequest(CreateError(result))
        };
    }

    private static object CreateError(FixtureOperationResult result)
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
