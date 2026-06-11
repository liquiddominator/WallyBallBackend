using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WallyBallBackend.Application.Resultados;

namespace WallyBallBackend.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize(Roles = "ORGANIZADOR")]
[Tags("Resultados")]
[Route("api/v{version:apiVersion}/resultados")]
public sealed class ResultadosController : ControllerBase
{
    private readonly IResultadoService _resultadoService;
    private readonly IValidator<RegisterResultadoRequest> _registerValidator;
    private readonly IValidator<UpdateResultadoRequest> _updateValidator;

    public ResultadosController(
        IResultadoService resultadoService,
        IValidator<RegisterResultadoRequest> registerValidator,
        IValidator<UpdateResultadoRequest> updateValidator)
    {
        _resultadoService = resultadoService;
        _registerValidator = registerValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ResultadoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetResultados(
        [FromQuery] int? campeonatoCategoriaId,
        CancellationToken cancellationToken)
    {
        var resultados = await _resultadoService.GetResultadosAsync(campeonatoCategoriaId, cancellationToken);

        return Ok(resultados);
    }

    [HttpGet("{resultadoId:int}")]
    [ProducesResponseType(typeof(ResultadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResultadoById(int resultadoId, CancellationToken cancellationToken)
    {
        var resultado = await _resultadoService.GetResultadoByIdAsync(resultadoId, cancellationToken);

        return resultado is null ? NotFound() : Ok(resultado);
    }

    [HttpGet("~/api/v{version:apiVersion}/partidos/{partidoId:int}/resultado")]
    [ProducesResponseType(typeof(ResultadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResultadoByPartido(int partidoId, CancellationToken cancellationToken)
    {
        var resultado = await _resultadoService.GetResultadoByPartidoAsync(partidoId, cancellationToken);

        return resultado is null ? NotFound() : Ok(resultado);
    }

    [HttpPost("~/api/v{version:apiVersion}/partidos/{partidoId:int}/resultado")]
    [ProducesResponseType(typeof(ResultadoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterResultado(
        int partidoId,
        RegisterResultadoRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(CreateValidationError(validationResult.ToDictionary()));
        }

        var result = await _resultadoService.RegisterResultadoAsync(partidoId, request, cancellationToken);

        if (!result.Succeeded)
        {
            return CreateErrorResponse(result);
        }

        var location = $"/api/v1/resultados/{result.Resultado!.IdResultado}";

        return Created(location, result.Resultado);
    }

    [HttpPut("{resultadoId:int}")]
    [ProducesResponseType(typeof(ResultadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateResultado(
        int resultadoId,
        UpdateResultadoRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(CreateValidationError(validationResult.ToDictionary()));
        }

        var result = await _resultadoService.UpdateResultadoAsync(resultadoId, request, cancellationToken);

        if (!result.Succeeded)
        {
            return CreateErrorResponse(result);
        }

        return Ok(result.Resultado);
    }

    [HttpGet("{resultadoId:int}/auditoria")]
    [ProducesResponseType(typeof(IReadOnlyCollection<AuditoriaResultadoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuditoriaResultado(int resultadoId, CancellationToken cancellationToken)
    {
        var auditoria = await _resultadoService.GetAuditoriaResultadoAsync(resultadoId, cancellationToken);

        return Ok(auditoria);
    }

    private IActionResult CreateErrorResponse(ResultadoOperationResult result)
    {
        return result.ErrorCode switch
        {
            "match_not_found" => NotFound(CreateError(result)),
            "result_not_found" => NotFound(CreateError(result)),
            "inactive_championship" => Conflict(CreateError(result)),
            "match_cancelled" => Conflict(CreateError(result)),
            "result_already_exists" => Conflict(CreateError(result)),
            "invalid_result" => Conflict(CreateError(result)),
            _ => BadRequest(CreateError(result))
        };
    }

    private static object CreateError(ResultadoOperationResult result)
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

