namespace PersonasService.Domain.Entities;

public sealed class RefreshToken
{
    public int IdRefreshToken { get; set; }

    public int IdUsuario { get; set; }

    public Usuario? Usuario { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime FechaExpiracion { get; set; }

    public DateTime? FechaRevocacion { get; set; }

    public string? ReemplazadoPorTokenHash { get; set; }

    public bool EstaActivo => FechaRevocacion is null && FechaExpiracion > DateTime.UtcNow;
}
