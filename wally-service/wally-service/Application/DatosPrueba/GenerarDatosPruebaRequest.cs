namespace WallyBallBackend.Application.DatosPrueba;

public sealed record GenerarDatosPruebaRequest(
    int Categorias = 2,
    int EquiposPorCategoria = 4,
    int JugadoresPorEquipo = 8,
    bool GenerarFixture = true,
    bool RegistrarResultados = true,
    int? Seed = null);

