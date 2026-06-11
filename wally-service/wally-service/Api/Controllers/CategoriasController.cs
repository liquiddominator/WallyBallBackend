using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WallyBallBackend.Application.Categorias;

namespace WallyBallBackend.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize(Roles = "ORGANIZADOR")]
[Tags("Categorias")]
[Route("api/v{version:apiVersion}/categorias")]
public sealed class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _categoriaService;
    private readonly IValidator<CreateCategoriaRequest> _createValidator;
    private readonly IValidator<AddCategoriaCampeonatoRequest> _addToCampeonatoValidator;

    public CategoriasController(
        ICategoriaService categoriaService,
        IValidator<CreateCategoriaRequest> createValidator,
        IValidator<AddCategoriaCampeonatoRequest> addToCampeonatoValidator)
    {
        _categoriaService = categoriaService;
        _createValidator = createValidator;
        _addToCampeonatoValidator = addToCampeonatoValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<CategoriaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCategorias(
        [FromQuery] int? campeonatoId,
        CancellationToken cancellationToken)
    {
        var categorias = await _categoriaService.GetCategoriasAsync(campeonatoId, cancellationToken);

        return Ok(categorias);
    }

    [HttpGet("~/api/v{version:apiVersion}/campeonatos/{campeonatoId:int}/categorias")]
    [ProducesResponseType(typeof(IReadOnlyCollection<CategoriaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCategoriasByCampeonato(
        int campeonatoId,
        CancellationToken cancellationToken)
    {
        if (campeonatoId <= 0)
        {
            return BadRequest(CreateValidationError(new Dictionary<string, string[]>
            {
                ["campeonatoId"] = ["El campeonato es obligatorio."]
            }));
        }

        var categorias = await _categoriaService.GetCategoriasAsync(campeonatoId, cancellationToken);

        return Ok(categorias);
    }

    [HttpGet("{categoriaId:int}")]
    [ProducesResponseType(typeof(CategoriaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoriaById(int categoriaId, CancellationToken cancellationToken)
    {
        var categoria = await _categoriaService.GetCategoriaByIdAsync(categoriaId, cancellationToken);

        return categoria is null ? NotFound() : Ok(categoria);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CategoriaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCategoria(
        [FromBody] CreateCategoriaRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(CreateValidationError(validationResult.ToDictionary()));
        }

        var result = await _categoriaService.CreateCategoriaAsync(request, cancellationToken);

        if (!result.Succeeded)
        {
            return CreateErrorResponse(result);
        }

        var location = $"/api/v1/categorias/{result.Value!.IdCategoria}";

        return Created(location, result.Value);
    }

    [HttpPost("~/api/v{version:apiVersion}/campeonatos/{campeonatoId:int}/categorias")]
    [ProducesResponseType(typeof(CategoriaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddCategoriaToCampeonato(
        int campeonatoId,
        [FromBody] AddCategoriaCampeonatoRequest request,
        CancellationToken cancellationToken)
    {
        if (campeonatoId <= 0)
        {
            return BadRequest(CreateValidationError(new Dictionary<string, string[]>
            {
                ["campeonatoId"] = ["El campeonato es obligatorio."]
            }));
        }

        var validationResult = await _addToCampeonatoValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(CreateValidationError(validationResult.ToDictionary()));
        }

        var result = await _categoriaService.AddCategoriaToCampeonatoAsync(campeonatoId, request, cancellationToken);

        if (!result.Succeeded)
        {
            return CreateErrorResponse(result);
        }

        var location = $"/api/v1/campeonatos/{campeonatoId}/categorias/{result.Value!.IdCampeonatoCategoria}";

        return Created(location, result.Value);
    }

    private IActionResult CreateErrorResponse(CategoriaOperationResult result)
    {
        return result.ErrorCode switch
        {
            "championship_not_found" => NotFound(CreateError(result)),
            "category_not_found" => NotFound(CreateError(result)),
            "inactive_championship" => Conflict(CreateError(result)),
            "inactive_category" => Conflict(CreateError(result)),
            "duplicate_category" => Conflict(CreateError(result)),
            "duplicate_championship_category" => Conflict(CreateError(result)),
            _ => BadRequest(CreateError(result))
        };
    }

    private static object CreateError(CategoriaOperationResult result)
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
