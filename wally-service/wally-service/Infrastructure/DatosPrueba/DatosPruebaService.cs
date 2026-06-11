using Bogus;
using Microsoft.EntityFrameworkCore;
using WallyBallBackend.Application.DatosPrueba;
using WallyBallBackend.Domain.Entities;
using WallyBallBackend.Infrastructure.Persistence.SqlServer;

namespace WallyBallBackend.Infrastructure.DatosPrueba;

public sealed class DatosPruebaService : IDatosPruebaService
{
    private const string TipoRoundRobin = "ROUND_ROBIN";

    private readonly AppDbContext _dbContext;

    public DatosPruebaService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DatosPruebaOperationResult> GenerarDatosPruebaAsync(
        GenerarDatosPruebaRequest request,
        CancellationToken cancellationToken)
    {
        var seed = request.Seed ?? Random.Shared.Next(1, int.MaxValue);
        var randomizer = new Randomizer(seed);
        var faker = new Faker("es");
        var suffix = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        var usedCedulas = await _dbContext.Jugadores
            .AsNoTracking()
            .Select(jugador => jugador.Cedula)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, cancellationToken);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var campeonato = CreateCampeonato(faker, suffix);
            var categorias = CreateCategorias(request.Categorias, suffix);
            var createdCategories = new List<DatosPruebaCategoriaResponse>();
            var totalEquipos = 0;
            var totalJugadores = 0;
            var totalPartidos = 0;
            var totalResultados = 0;

            _dbContext.Campeonatos.Add(campeonato);
            _dbContext.Categorias.AddRange(categorias);
            await _dbContext.SaveChangesAsync(cancellationToken);

            foreach (var categoria in categorias)
            {
                var campeonatoCategoria = new CampeonatoCategoria
                {
                    IdCampeonato = campeonato.IdCampeonato,
                    IdCategoria = categoria.IdCategoria,
                    Estado = "ACTIVA"
                };

                _dbContext.CampeonatoCategorias.Add(campeonatoCategoria);
                await _dbContext.SaveChangesAsync(cancellationToken);

                var equipos = CreateEquipos(campeonatoCategoria.IdCampeonatoCategoria, categoria.Nombre, request.EquiposPorCategoria, faker);
                _dbContext.Equipos.AddRange(equipos);
                await _dbContext.SaveChangesAsync(cancellationToken);

                var jugadores = CreateJugadores(request.JugadoresPorEquipo * equipos.Count, faker, randomizer, usedCedulas);
                _dbContext.Jugadores.AddRange(jugadores);
                await _dbContext.SaveChangesAsync(cancellationToken);

                var inscripciones = CreateInscripciones(equipos, jugadores, request.JugadoresPorEquipo);
                _dbContext.InscripcionesEquipoJugador.AddRange(inscripciones);
                await _dbContext.SaveChangesAsync(cancellationToken);

                var partidos = new List<Partido>();

                if (request.GenerarFixture)
                {
                    var fase = CreateFixture(campeonatoCategoria.IdCampeonatoCategoria, equipos, campeonato.FechaInicio);
                    _dbContext.Fases.Add(fase);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    partidos = fase.Jornadas
                        .SelectMany(jornada => jornada.Partidos)
                        .ToList();
                }

                var resultados = new List<Resultado>();

                if (request.GenerarFixture && request.RegistrarResultados)
                {
                    resultados = CreateResultados(partidos, randomizer);
                    _dbContext.Resultados.AddRange(resultados);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                createdCategories.Add(new DatosPruebaCategoriaResponse(
                    categoria.IdCategoria,
                    categoria.Nombre,
                    campeonatoCategoria.IdCampeonatoCategoria,
                    equipos.Select(equipo => equipo.IdEquipo).ToList(),
                    partidos.Select(partido => partido.IdPartido).ToList(),
                    resultados.Select(resultado => resultado.IdResultado).ToList()));

                totalEquipos += equipos.Count;
                totalJugadores += jugadores.Count;
                totalPartidos += partidos.Count;
                totalResultados += resultados.Count;
            }

            await transaction.CommitAsync(cancellationToken);

            return DatosPruebaOperationResult.Success(new DatosPruebaResponse(
                campeonato.IdCampeonato,
                campeonato.Nombre,
                createdCategories,
                totalEquipos,
                totalJugadores,
                totalPartidos,
                totalResultados));
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return DatosPruebaOperationResult.Failure("demo_data_persistence_error", "No se pudieron generar los datos de prueba.");
        }
    }

    private static Campeonato CreateCampeonato(Faker faker, string suffix)
    {
        var fechaInicio = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(7));

        return new Campeonato
        {
            Nombre = $"Torneo {faker.Address.City()} {suffix}",
            FechaInicio = fechaInicio,
            FechaFin = fechaInicio.AddDays(45),
            Estado = "ACTIVO"
        };
    }

    private static List<Categoria> CreateCategorias(int count, string suffix)
    {
        var baseNames = new[]
        {
            "Varones Libre",
            "Damas Libre",
            "Mixto Libre",
            "Juvenil"
        };

        return baseNames
            .Take(count)
            .Select(name => new Categoria
            {
                Nombre = $"{name} {suffix}",
                Estado = "ACTIVA"
            })
            .ToList();
    }

    private static List<Equipo> CreateEquipos(
        int campeonatoCategoriaId,
        string categoria,
        int count,
        Faker faker)
    {
        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var equipos = new List<Equipo>();

        while (equipos.Count < count)
        {
            var name = $"{faker.Address.City()} {faker.Commerce.Color()}";

            if (!usedNames.Add(name))
            {
                continue;
            }

            equipos.Add(new Equipo
            {
                IdCampeonatoCategoria = campeonatoCategoriaId,
                Nombre = name.Length <= 120 ? name : $"{categoria} {equipos.Count + 1}",
                Activo = true
            });
        }

        return equipos;
    }

    private static List<Jugador> CreateJugadores(
        int count,
        Faker faker,
        Randomizer randomizer,
        ISet<string> usedCedulas)
    {
        var jugadores = new List<Jugador>();

        while (jugadores.Count < count)
        {
            var cedula = randomizer.Replace("########");

            if (!usedCedulas.Add(cedula))
            {
                continue;
            }

            jugadores.Add(new Jugador
            {
                Cedula = cedula,
                Nombre = faker.Name.FirstName(),
                Apellido = faker.Name.LastName(),
                Telefono = $"7{randomizer.Replace("#######")}",
                FechaNacimiento = DateOnly.FromDateTime(faker.Date.Past(20, DateTime.UtcNow.AddYears(-16))),
                Activo = true
            });
        }

        return jugadores;
    }

    private static List<InscripcionEquipoJugador> CreateInscripciones(
        IReadOnlyList<Equipo> equipos,
        IReadOnlyList<Jugador> jugadores,
        int jugadoresPorEquipo)
    {
        var inscripciones = new List<InscripcionEquipoJugador>();
        var jugadorIndex = 0;

        foreach (var equipo in equipos)
        {
            for (var index = 0; index < jugadoresPorEquipo; index++)
            {
                inscripciones.Add(new InscripcionEquipoJugador
                {
                    IdEquipo = equipo.IdEquipo,
                    IdJugador = jugadores[jugadorIndex].IdJugador,
                    Estado = "ACTIVO"
                });

                jugadorIndex++;
            }
        }

        return inscripciones;
    }

    private static Fase CreateFixture(
        int campeonatoCategoriaId,
        IReadOnlyList<Equipo> equipos,
        DateOnly fechaInicio)
    {
        var fase = new Fase
        {
            IdCampeonatoCategoria = campeonatoCategoriaId,
            Nombre = "Todos contra todos",
            Tipo = TipoRoundRobin,
            Orden = 1,
            Estado = "ACTIVA"
        };

        var rotacion = equipos.Cast<Equipo?>().ToList();

        if (rotacion.Count % 2 != 0)
        {
            rotacion.Add(null);
        }

        var totalEquipos = rotacion.Count;
        var totalJornadas = totalEquipos - 1;

        for (var numeroJornada = 1; numeroJornada <= totalJornadas; numeroJornada++)
        {
            var fechaJornada = fechaInicio.AddDays((numeroJornada - 1) * 7);
            var jornada = new Jornada
            {
                Fase = fase,
                NumeroJornada = numeroJornada,
                FechaProgramada = fechaJornada,
                Estado = "PROGRAMADA"
            };

            for (var index = 0; index < totalEquipos / 2; index++)
            {
                var local = rotacion[index];
                var visitante = rotacion[totalEquipos - 1 - index];

                if (local is null || visitante is null)
                {
                    continue;
                }

                if (numeroJornada % 2 == 0)
                {
                    (local, visitante) = (visitante, local);
                }

                jornada.Partidos.Add(new Partido
                {
                    IdCampeonatoCategoria = campeonatoCategoriaId,
                    Fase = fase,
                    Jornada = jornada,
                    IdEquipoLocal = local.IdEquipo,
                    IdEquipoVisitante = visitante.IdEquipo,
                    FechaHoraProgramada = fechaJornada.ToDateTime(new TimeOnly(20, 0)),
                    Estado = "PROGRAMADO"
                });
            }

            fase.Jornadas.Add(jornada);

            var last = rotacion[^1];
            rotacion.RemoveAt(rotacion.Count - 1);
            rotacion.Insert(1, last);
        }

        return fase;
    }

    private static List<Resultado> CreateResultados(
        IReadOnlyList<Partido> partidos,
        Randomizer randomizer)
    {
        return partidos
            .Select(partido => CreateResultado(partido, randomizer))
            .ToList();
    }

    private static Resultado CreateResultado(Partido partido, Randomizer randomizer)
    {
        var localWins = randomizer.Bool();
        var sets = localWins
            ? CreateWinningSets(localTeamWins: true, randomizer)
            : CreateWinningSets(localTeamWins: false, randomizer);
        var setsLocal = sets.Count(set => set.PuntosLocal > set.PuntosVisitante);
        var setsVisitante = sets.Count(set => set.PuntosVisitante > set.PuntosLocal);

        return new Resultado
        {
            IdPartido = partido.IdPartido,
            SetsLocal = setsLocal,
            SetsVisitante = setsVisitante,
            IdEquipoGanador = localWins ? partido.IdEquipoLocal : partido.IdEquipoVisitante,
            FechaRegistro = DateTime.UtcNow,
            Sets = sets
        };
    }

    private static List<ResultadoSet> CreateWinningSets(bool localTeamWins, Randomizer randomizer)
    {
        var loserSetWins = randomizer.Int(0, 1);
        var totalSets = 2 + loserSetWins;
        var winningSetNumbers = Enumerable.Range(1, totalSets)
            .OrderBy(_ => randomizer.Int())
            .Take(2)
            .ToHashSet();
        var sets = new List<ResultadoSet>();

        for (var numeroSet = 1; numeroSet <= totalSets; numeroSet++)
        {
            var winnerWinsSet = winningSetNumbers.Contains(numeroSet);
            var localWinsSet = localTeamWins ? winnerWinsSet : !winnerWinsSet;
            var winnerPoints = numeroSet == 3 ? 15 : 25;
            var loserPoints = numeroSet == 3
                ? randomizer.Int(8, 13)
                : randomizer.Int(16, 23);

            sets.Add(new ResultadoSet
            {
                NumeroSet = numeroSet,
                PuntosLocal = localWinsSet ? winnerPoints : loserPoints,
                PuntosVisitante = localWinsSet ? loserPoints : winnerPoints
            });
        }

        return sets;
    }
}
