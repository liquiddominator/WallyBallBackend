namespace IdentidadService.Domain.Entities;

public sealed class Usuario
{
    public int IdUsuario { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string? NombreCompleto { get; set; }

    public bool Activo { get; set; } = true;

    public int AccessFailedCount { get; set; }

    public DateTime? LockoutEndUtc { get; set; }

    public DateTime? PasswordChangedAtUtc { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = [];

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
