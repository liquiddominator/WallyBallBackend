namespace WallyBallBackend.Application.DatosPrueba;

public sealed record DatosPruebaResponse(
    int IdCampeonato,
    string Campeonato,
    IReadOnlyCollection<DatosPruebaCategoriaResponse> Categorias,
    int EquiposCreados,
    int JugadoresCreados,
    int PartidosCreados,
    int ResultadosCreados);

public sealed record DatosPruebaCategoriaResponse(
    int IdCategoria,
    string Categoria,
    int IdCampeonatoCategoria,
    IReadOnlyCollection<int> IdsEquipos,
    IReadOnlyCollection<int> IdsPartidos,
    IReadOnlyCollection<int> IdsResultados);

