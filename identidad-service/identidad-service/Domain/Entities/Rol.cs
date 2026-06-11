namespace IdentidadService.Domain.Entities;

public sealed class Rol
{
    public int IdRol { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = [];
}
