using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WallyBallBackend.Application.DatosPrueba;

namespace WallyBallBackend.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize(Roles = "ORGANIZADOR")]
[Tags("Datos de prueba")]
[Route("api/v{version:apiVersion}/dev/datos-prueba")]
public sealed class DatosPruebaController : ControllerBase
{
    private readonly IDatosPruebaService _datosPruebaService;
    private readonly IValidator<GenerarDatosPruebaRequest> _validator;
    private readonly IWebHostEnvironment _environment;

    public DatosPruebaController(
        IDatosPruebaService datosPruebaService,
        IValidator<GenerarDatosPruebaRequest> validator,
        IWebHostEnvironment environment)
    {
        _datosPruebaService = datosPruebaService;
        _validator = validator;
        _environment = environment;
    }

    [HttpPost]
    [ProducesResponseType(typeof(DatosPruebaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GenerarDatosPrueba(
        [FromBody] GenerarDatosPruebaRequest? request,
        CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment() && !_environment.IsEnvironment("Docker"))
        {
            return NotFound();
        }

        request ??= new GenerarDatosPruebaRequest();

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(CreateValidationError(validationResult.ToDictionary()));
        }

        var result = await _datosPruebaService.GenerarDatosPruebaAsync(request, cancellationToken);

        if (!result.Succeeded)
        {
            return Conflict(new
            {
                code = result.ErrorCode,
                message = result.ErrorMessage
            });
        }

        return Created($"/api/v1/campeonatos/{result.Value!.IdCampeonato}", result.Value);
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

