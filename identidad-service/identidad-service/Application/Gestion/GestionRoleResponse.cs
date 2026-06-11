namespace IdentidadService.Application.Gestion;

public sealed record GestionRoleResponse(
    int IdRol,
    string Nombre,
    string? Descripcion,
    bool Activo);
