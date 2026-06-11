using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WallyBallBackend.Application.Campeonatos;

namespace WallyBallBackend.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize(Roles = "ORGANIZADOR")]
[Tags("Campeonatos")]
[Route("api/v{version:apiVersion}/campeonatos")]
public sealed class CampeonatosController : ControllerBase
{
    private readonly ICampeonatoService _campeonatoService;
    private readonly IValidator<CreateCampeonatoRequest> _createValidator;
    private readonly IValidator<UpdateCampeonatoRequest> _updateValidator;

    public CampeonatosController(
        ICampeonatoService campeonatoService,
        IValidator<CreateCampeonatoRequest> createValidator,
        IValidator<UpdateCampeonatoRequest> updateValidator)
    {
        _campeonatoService = campeonatoService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<CampeonatoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCampeonatos(CancellationToken cancellationToken)
    {
        var campeonatos = await _campeonatoService.GetCampeonatosAsync(cancellationToken);

        return Ok(campeonatos);
    }

    [HttpGet("{campeonatoId:int}")]
    [ProducesResponseType(typeof(CampeonatoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCampeonatoById(int campeonatoId, CancellationToken cancellationToken)
    {
        var campeonato = await _campeonatoService.GetCampeonatoByIdAsync(campeonatoId, cancellationToken);

        return campeonato is null ? NotFound() : Ok(campeonato);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CampeonatoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateCampeonato(CreateCampeonatoRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(CreateValidationError(validationResult.ToDictionary()));
        }

        var result = await _campeonatoService.CreateCampeonatoAsync(request, cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(CreateError(result));
        }

        return CreatedAtAction(
            nameof(GetCampeonatoById),
            new { campeonatoId = result.Value!.IdCampeonato, version = "1" },
            result.Value);
    }

    [HttpPut("{campeonatoId:int}")]
    [ProducesResponseType(typeof(CampeonatoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateCampeonato(
        int campeonatoId,
        UpdateCampeonatoRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(CreateValidationError(validationResult.ToDictionary()));
        }

        var result = await _campeonatoService.UpdateCampeonatoAsync(campeonatoId, request, cancellationToken);

        return CreateResultResponse(result);
    }

    [HttpPatch("{campeonatoId:int}/finalizar")]
    [ProducesResponseType(typeof(CampeonatoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> FinalizeCampeonato(int campeonatoId, CancellationToken cancellationToken)
    {
        var result = await _campeonatoService.FinalizeCampeonatoAsync(campeonatoId, cancellationToken);

        return CreateResultResponse(result);
    }

    private IActionResult CreateResultResponse(CampeonatoOperationResult result)
    {
        if (result.Succeeded)
        {
            return Ok(result.Value);
        }

        return result.ErrorCode switch
        {
            "not_found" => NotFound(CreateError(result)),
            "inactive_championship" => Conflict(CreateError(result)),
            _ => BadRequest(CreateError(result))
        };
    }

    private static object CreateError(CampeonatoOperationResult result)
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
