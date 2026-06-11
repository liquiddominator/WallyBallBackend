using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using PersonasService.Application.Auth;

namespace PersonasService.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize]
[EnableRateLimiting("auth")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshTokenValidator;
    private readonly IValidator<LogoutRequest> _logoutValidator;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<RefreshTokenRequest> refreshTokenValidator,
        IValidator<LogoutRequest> logoutValidator,
        IValidator<ChangePasswordRequest> changePasswordValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _refreshTokenValidator = refreshTokenValidator;
        _logoutValidator = logoutValidator;
        _changePasswordValidator = changePasswordValidator;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(CreateValidationError(validationResult.ToDictionary()));
        }

        var result = await _authService.RegisterAsync(request, cancellationToken);

        if (!result.Succeeded)
        {
            return result.ErrorCode == "duplicate_email"
                ? Conflict(CreateError(result))
                : BadRequest(CreateError(result));
        }

        return CreatedAtAction(nameof(Register), result.Value);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(CreateValidationError(validationResult.ToDictionary()));
        }

        var result = await _authService.LoginAsync(request, cancellationToken);

        if (!result.Succeeded)
        {
            return result.ErrorCode == "account_locked"
                ? StatusCode(StatusCodes.Status423Locked, CreateError(result))
                : Unauthorized(CreateError(result));
        }

        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _refreshTokenValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(CreateValidationError(validationResult.ToDictionary()));
        }

        var result = await _authService.RefreshAsync(request, cancellationToken);

        if (!result.Succeeded)
        {
            return Unauthorized(CreateError(result));
        }

        return Ok(result.Value);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(AuthUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(CreateError(AuthResult.Failure("invalid_token", "Token invalido.")));
        }

        var user = await _authService.GetCurrentUserAsync(userId, cancellationToken);

        if (user is null)
        {
            return Unauthorized(CreateError(AuthResult.Failure("invalid_token", "Usuario no encontrado o inactivo.")));
        }

        return Ok(user);
    }

    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _changePasswordValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(CreateValidationError(validationResult.ToDictionary()));
        }

        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(CreateError(AuthOperationResult.Failure("invalid_token", "Token invalido.")));
        }

        var result = await _authService.ChangePasswordAsync(userId, request, cancellationToken);

        if (!result.Succeeded)
        {
            return result.ErrorCode == "invalid_credentials"
                ? Unauthorized(CreateError(result))
                : BadRequest(CreateError(result));
        }

        return NoContent();
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(LogoutRequest? request, CancellationToken cancellationToken)
    {
        request ??= new LogoutRequest(null);
        var validationResult = await _logoutValidator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(CreateValidationError(validationResult.ToDictionary()));
        }

        await _authService.LogoutAsync(request, cancellationToken);

        return NoContent();
    }

    private static object CreateError(AuthResult result)
    {
        return new
        {
            code = result.ErrorCode,
            message = result.ErrorMessage
        };
    }

    private static object CreateError(AuthOperationResult result)
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

    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("nameid") ??
            User.FindFirstValue("sub");

        return int.TryParse(userIdClaim, out userId);
    }
}
