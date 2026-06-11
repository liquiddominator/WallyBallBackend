namespace WallyBallBackend.Application.Categorias;

public sealed record CategoriaResponse(
    int IdCategoria,
    int? IdCampeonatoCategoria,
    int? IdCampeonato,
    string? CampeonatoNombre,
    string Nombre,
    string Estado,
    DateTime FechaCreacion,
    DateTime? FechaActualizacion);
