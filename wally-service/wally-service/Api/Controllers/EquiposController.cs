using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WallyBallBackend.Application.Equipos;

namespace WallyBallBackend.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize(Roles = "ORGANIZADOR")]
[Tags("Equipos")]
[Route("api/v{version:apiVersion}/equipos")]
public sealed class EquiposController : ControllerBase
{
    private readonly IEquipoService _equipoService;
    private readonly IValidator<CreateEquipoRequest> _createValidator;
    private readonly IValidator<UpdateEquipoRequest> _updateValidator;

    public EquiposController(
        IEquipoService equipoService,
        IValidator<CreateEquipoRequest> createValidator,
        IValidator<UpdateEquipoRequest> updateValidator)
    {
        _equipoService = equipoService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<EquipoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEquipos(
        [FromQuery] int? campeonatoCategoriaId,
        CancellationToken cancellationToken)
    {
        var equipos = await _equipoService.GetEquiposAsync(campeonatoCategoriaId, cancellationToken);

        return Ok(equipos);
    }

    [HttpGet("~/api/v{version:apiVersion}/campeonatos-categorias/{campeonatoCategoriaId:int}/equipos")]
    [ProducesResponseType(typeof(IReadOnlyCollection<EquipoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEquiposByCampeonatoCategoria(
        int campeonatoCategoriaId,
        CancellationToken cancellationToken)
    {
        if (campeonatoCategoriaId <= 0)
        {
            return BadRequest(CreateValidationError(new Dictionary<string, string[]>
            {
                ["campeonatoCategoriaId"] = ["La categoria del campeonato es obligatoria."]
            }));
        }

        var equipos = await _equipoService.GetEquiposAsync(campeonatoCategoriaId, cancellationToken);

        return Ok(equipos);
    }

    [HttpGet("{equipoId:int}")]
    [ProducesResponseType(typeof(EquipoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEquipoById(int equipoId, CancellationToken cancellationToken)
    {
        var equipo = await _equipoService.GetEquipoByIdAsync(equipoId, cancellationToken);

        return equipo is null ? NotFound() : Ok(equipo);
    }

    [HttpPost]
    [ProducesResponseType(typeof(EquipoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateEquipo(
        [FromHeader(Name = "IdCampeonatoCategoria")] int? campeonatoCategoriaId,
        [FromBody] CreateEquipoRequest request,
        CancellationToken cancellationToken)
    {
        if (!campeonatoCategoriaId.HasValue || campeonatoCategoriaId.Value <= 0)
        {
            return BadRequest(CreateValidationError(new Dictionary<string, string[]>
            {
                ["IdCampeonatoCategoria"] = ["El header IdCampeonatoCategoria es obligatorio."]
            }));
        }

        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(CreateValidationError(validationResult.ToDictionary()));
        }

        var result = await _equipoService.CreateEquipoAsync(campeonatoCategoriaId.Value, request, cancellationToken);

        if (!result.Succeeded)
        {
            return CreateErrorResponse(result);
        }

        var location = $"/api/v1/equipos/{result.Value!.IdEquipo}";

        return Created(location, result.Value);
    }

    [HttpPut("{equipoId:int}")]
    [ProducesResponseType(typeof(EquipoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateEquipo(
        int equipoId,
        UpdateEquipoRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(CreateValidationError(validationResult.ToDictionary()));
        }

        var result = await _equipoService.UpdateEquipoAsync(equipoId, request, cancellationToken);

        if (!result.Succeeded)
        {
            return CreateErrorResponse(result);
        }

        return Ok(result.Value);
    }

    private IActionResult CreateErrorResponse(EquipoOperationResult result)
    {
        return result.ErrorCode switch
        {
            "category_not_found" => NotFound(CreateError(result)),
            "championship_category_not_found" => NotFound(CreateError(result)),
            "not_found" => NotFound(CreateError(result)),
            "inactive_championship" => Conflict(CreateError(result)),
            "duplicate_team" => Conflict(CreateError(result)),
            _ => BadRequest(CreateError(result))
        };
    }

    private static object CreateError(EquipoOperationResult result)
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
