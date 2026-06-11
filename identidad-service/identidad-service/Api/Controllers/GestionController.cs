using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IdentidadService.Application.Gestion;

namespace IdentidadService.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize(Roles = "ORGANIZADOR")]
[Tags("Gestion")]
[Route("api/v{version:apiVersion}/gestion")]
public sealed class GestionController : ControllerBase
{
    private readonly IGestionQueryService _gestionQueryService;

    public GestionController(IGestionQueryService gestionQueryService)
    {
        _gestionQueryService = gestionQueryService;
    }

    [HttpGet("roles")]
    [ProducesResponseType(typeof(IReadOnlyCollection<GestionRoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var roles = await _gestionQueryService.GetRolesAsync(cancellationToken);

        return Ok(roles);
    }

    [HttpGet("roles/{roleId:int}")]
    [ProducesResponseType(typeof(GestionRoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleById(int roleId, CancellationToken cancellationToken)
    {
        var role = await _gestionQueryService.GetRoleByIdAsync(roleId, cancellationToken);

        return role is null ? NotFound() : Ok(role);
    }

    [HttpGet("usuarios")]
    [ProducesResponseType(typeof(IReadOnlyCollection<GestionUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _gestionQueryService.GetUsersAsync(cancellationToken);

        return Ok(users);
    }

    [HttpGet("usuarios/{userId:int}")]
    [ProducesResponseType(typeof(GestionUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(int userId, CancellationToken cancellationToken)
    {
        var user = await _gestionQueryService.GetUserByIdAsync(userId, cancellationToken);

        return user is null ? NotFound() : Ok(user);
    }
}
