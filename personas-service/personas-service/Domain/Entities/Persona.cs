namespace PersonasService.Domain.Entities;

public sealed class Persona
{
    public int IdPersona { get; set; }

    public string Cedula { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<Usuario> Usuarios { get; set; } = [];
}
