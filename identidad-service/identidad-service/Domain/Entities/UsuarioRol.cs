namespace IdentidadService.Domain.Entities;

public sealed class UsuarioRol
{
    public int IdUsuarioRol { get; set; }

    public int IdUsuario { get; set; }

    public Usuario? Usuario { get; set; }

    public int IdRol { get; set; }

    public Rol? Rol { get; set; }

    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
}
