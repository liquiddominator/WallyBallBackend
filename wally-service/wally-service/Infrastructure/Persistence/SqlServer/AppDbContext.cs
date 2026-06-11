using Microsoft.EntityFrameworkCore;
using WallyBallBackend.Domain.Entities;

namespace WallyBallBackend.Infrastructure.Persistence.SqlServer;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Campeonato> Campeonatos => Set<Campeonato>();

    public DbSet<Categoria> Categorias => Set<Categoria>();

    public DbSet<CampeonatoCategoria> CampeonatoCategorias => Set<CampeonatoCategoria>();

    public DbSet<Equipo> Equipos => Set<Equipo>();

    public DbSet<Jugador> Jugadores => Set<Jugador>();

    public DbSet<InscripcionEquipoJugador> InscripcionesEquipoJugador => Set<InscripcionEquipoJugador>();

    public DbSet<Fase> Fases => Set<Fase>();

    public DbSet<Jornada> Jornadas => Set<Jornada>();

    public DbSet<Partido> Partidos => Set<Partido>();

    public DbSet<Resultado> Resultados => Set<Resultado>();

    public DbSet<ResultadoSet> ResultadoSets => Set<ResultadoSet>();

    public DbSet<TablaPosicion> TablaPosiciones => Set<TablaPosicion>();

    public DbSet<AuditoriaResultado> AuditoriaResultados => Set<AuditoriaResultado>();

    public DbSet<ReprogramacionPartido> ReprogramacionesPartido => Set<ReprogramacionPartido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureJugadores(modelBuilder);
        ConfigureCampeonatos(modelBuilder);
        ConfigureFixture(modelBuilder);
        ConfigureResultados(modelBuilder);
    }

    private static void ConfigureJugadores(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Jugador>(builder =>
        {
            builder.ToTable("Jugadores");
            builder.HasKey(jugador => jugador.IdJugador);
            builder.Property(jugador => jugador.Cedula).HasMaxLength(30).IsRequired();
            builder.Property(jugador => jugador.Nombre).HasMaxLength(100).IsRequired();
            builder.Property(jugador => jugador.Apellido).HasMaxLength(100).IsRequired();
            builder.Property(jugador => jugador.Telefono).HasMaxLength(30);
            builder.Property(jugador => jugador.Activo).HasDefaultValue(true);
            builder.Property(jugador => jugador.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");
            builder.HasIndex(jugador => jugador.Cedula).IsUnique().HasDatabaseName("UQ_Jugadores_Cedula");
        });

        modelBuilder.Entity<InscripcionEquipoJugador>(builder =>
        {
            builder.ToTable("InscripcionesEquipoJugador", table =>
            {
                table.HasCheckConstraint("CK_Inscripciones_Estado", "Estado IN ('ACTIVO', 'RETIRADO')");
                table.HasTrigger("TRG_Maximo12JugadoresPorEquipo");
            });
            builder.HasKey(inscripcion => inscripcion.IdInscripcion);
            builder.Property(inscripcion => inscripcion.FechaInscripcion).HasDefaultValueSql("SYSUTCDATETIME()");
            builder.Property(inscripcion => inscripcion.Estado).HasMaxLength(30).HasDefaultValue("ACTIVO").IsRequired();
            builder.HasIndex(inscripcion => new { inscripcion.IdEquipo, inscripcion.IdJugador })
                .IsUnique()
                .HasDatabaseName("UQ_Inscripciones_Equipo_Jugador");
            builder.HasOne(inscripcion => inscripcion.Equipo)
                .WithMany(equipo => equipo.Inscripciones)
                .HasForeignKey(inscripcion => inscripcion.IdEquipo)
                .OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(inscripcion => inscripcion.Jugador)
                .WithMany(jugador => jugador.Inscripciones)
                .HasForeignKey(inscripcion => inscripcion.IdJugador)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }

    private static void ConfigureCampeonatos(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Campeonato>(builder =>
        {
            builder.ToTable("Campeonatos", table =>
            {
                table.HasCheckConstraint("CK_Campeonatos_Estado", "Estado IN ('BORRADOR', 'ACTIVO', 'FINALIZADO', 'CANCELADO')");
                table.HasCheckConstraint("CK_Campeonatos_Fechas", "FechaFin IS NULL OR FechaFin >= FechaInicio");
            });
            builder.HasKey(campeonato => campeonato.IdCampeonato);
            builder.Property(campeonato => campeonato.Nombre).HasMaxLength(150).IsRequired();
            builder.Property(campeonato => campeonato.Estado).HasMaxLength(30).HasDefaultValue("ACTIVO").IsRequired();
            builder.Property(campeonato => campeonato.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        modelBuilder.Entity<Categoria>(builder =>
        {
            builder.ToTable("Categorias", table =>
            {
                table.HasCheckConstraint("CK_Categorias_Estado", "Estado IN ('ACTIVA', 'INACTIVA')");
            });
            builder.HasKey(categoria => categoria.IdCategoria);
            builder.Property(categoria => categoria.Nombre).HasMaxLength(80).IsRequired();
            builder.Property(categoria => categoria.Estado).HasMaxLength(30).HasDefaultValue("ACTIVA").IsRequired();
            builder.Property(categoria => categoria.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");
            builder.HasIndex(categoria => categoria.Nombre)
                .IsUnique()
                .HasDatabaseName("UQ_Categorias_Nombre");
        });

        modelBuilder.Entity<CampeonatoCategoria>(builder =>
        {
            builder.ToTable("CampeonatosCategorias", table =>
            {
                table.HasCheckConstraint("CK_CampeonatosCategorias_Estado", "Estado IN ('ACTIVA', 'INACTIVA')");
                table.HasTrigger("TRG_CampeonatosCategorias_BloquearCampeonatoFinalizado");
            });
            builder.HasKey(campeonatoCategoria => campeonatoCategoria.IdCampeonatoCategoria);
            builder.Property(campeonatoCategoria => campeonatoCategoria.Estado).HasMaxLength(30).HasDefaultValue("ACTIVA").IsRequired();
            builder.Property(campeonatoCategoria => campeonatoCategoria.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");
            builder.HasIndex(campeonatoCategoria => new { campeonatoCategoria.IdCampeonato, campeonatoCategoria.IdCategoria })
                .IsUnique()
                .HasDatabaseName("UQ_CampeonatosCategorias_Campeonato_Categoria");
            builder.HasOne(campeonatoCategoria => campeonatoCategoria.Campeonato)
                .WithMany(campeonato => campeonato.CampeonatoCategorias)
                .HasForeignKey(campeonatoCategoria => campeonatoCategoria.IdCampeonato)
                .OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(campeonatoCategoria => campeonatoCategoria.Categoria)
                .WithMany(categoria => categoria.CampeonatoCategorias)
                .HasForeignKey(campeonatoCategoria => campeonatoCategoria.IdCategoria)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Equipo>(builder =>
        {
            builder.ToTable("Equipos", table =>
            {
                table.HasTrigger("TRG_Equipos_BloquearCampeonatoFinalizado");
            });
            builder.HasKey(equipo => equipo.IdEquipo);
            builder.Property(equipo => equipo.Nombre).HasMaxLength(120).IsRequired();
            builder.Property(equipo => equipo.Activo).HasDefaultValue(true);
            builder.Property(equipo => equipo.FechaCreacion).HasDefaultValueSql("SYSUTCDATETIME()");
            builder.HasIndex(equipo => new { equipo.IdCampeonatoCategoria, equipo.Nombre })
                .IsUnique()
                .HasDatabaseName("UQ_Equipos_CampeonatoCategoria_Nombre");
            builder.HasOne(equipo => equipo.CampeonatoCategoria)
                .WithMany(campeonatoCategoria => campeonatoCategoria.Equipos)
                .HasForeignKey(equipo => equipo.IdCampeonatoCategoria)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }

    private static void ConfigureFixture(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Fase>(builder =>
        {
            builder.ToTable("Fases", table =>
            {
                table.HasCheckConstraint("CK_Fases_Tipo", "Tipo IN ('ROUND_ROBIN', 'ELIMINATORIA')");
                table.HasCheckConstraint("CK_Fases_Estado", "Estado IN ('PENDIENTE', 'ACTIVA', 'FINALIZADA', 'CANCELADA')");
            });
            builder.HasKey(fase => fase.IdFase);
            builder.Property(fase => fase.Nombre).HasMaxLength(80).IsRequired();
            builder.Property(fase => fase.Tipo).HasMaxLength(30).IsRequired();
            builder.Property(fase => fase.Estado).HasMaxLength(30).HasDefaultValue("PENDIENTE").IsRequired();
            builder.HasIndex(fase => new { fase.IdCampeonatoCategoria, fase.Orden })
                .IsUnique()
                .HasDatabaseName("UQ_Fases_CampeonatoCategoria_Orden");
            builder.HasOne(fase => fase.CampeonatoCategoria)
                .WithMany(campeonatoCategoria => campeonatoCategoria.Fases)
                .HasForeignKey(fase => fase.IdCampeonatoCategoria)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Jornada>(builder =>
        {
            builder.ToTable("Jornadas", table =>
            {
                table.HasCheckConstraint("CK_Jornadas_Numero", "NumeroJornada > 0");
                table.HasCheckConstraint("CK_Jornadas_Estado", "Estado IN ('PROGRAMADA', 'FINALIZADA', 'CANCELADA')");
            });
            builder.HasKey(jornada => jornada.IdJornada);
            builder.Property(jornada => jornada.Estado).HasMaxLength(30).HasDefaultValue("PROGRAMADA").IsRequired();
            builder.HasIndex(jornada => new { jornada.IdFase, jornada.NumeroJornada })
                .IsUnique()
                .HasDatabaseName("UQ_Jornadas_Fase_Numero");
            builder.HasOne(jornada => jornada.Fase)
                .WithMany(fase => fase.Jornadas)
                .HasForeignKey(jornada => jornada.IdFase)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Partido>(builder =>
        {
            builder.ToTable("Partidos", table =>
            {
                table.HasCheckConstraint("CK_Partidos_EquiposDiferentes", "IdEquipoLocal <> IdEquipoVisitante");
                table.HasCheckConstraint("CK_Partidos_Estado", "Estado IN ('PROGRAMADO', 'REPROGRAMADO', 'FINALIZADO', 'CANCELADO')");
                table.HasTrigger("TRG_Partidos_ValidarIntegridad");
            });
            builder.HasKey(partido => partido.IdPartido);
            builder.Property(partido => partido.Estado).HasMaxLength(30).HasDefaultValue("PROGRAMADO").IsRequired();
            builder.Property<int>("EquipoMenor").HasComputedColumnSql("CASE WHEN IdEquipoLocal < IdEquipoVisitante THEN IdEquipoLocal ELSE IdEquipoVisitante END", stored: true);
            builder.Property<int>("EquipoMayor").HasComputedColumnSql("CASE WHEN IdEquipoLocal > IdEquipoVisitante THEN IdEquipoLocal ELSE IdEquipoVisitante END", stored: true);
            builder.HasIndex(partido => new { partido.IdFase, partido.IdEquipoLocal, partido.IdEquipoVisitante });
            builder.HasIndex("IdFase", "EquipoMenor", "EquipoMayor")
                .IsUnique()
                .HasDatabaseName("UQ_Partidos_Fase_Enfrentamiento");
            builder.HasOne(partido => partido.CampeonatoCategoria)
                .WithMany(campeonatoCategoria => campeonatoCategoria.Partidos)
                .HasForeignKey(partido => partido.IdCampeonatoCategoria)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(partido => partido.Fase)
                .WithMany(fase => fase.Partidos)
                .HasForeignKey(partido => partido.IdFase)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(partido => partido.Jornada)
                .WithMany(jornada => jornada.Partidos)
                .HasForeignKey(partido => partido.IdJornada)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(partido => partido.EquipoLocal)
                .WithMany(equipo => equipo.PartidosComoLocal)
                .HasForeignKey(partido => partido.IdEquipoLocal)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(partido => partido.EquipoVisitante)
                .WithMany(equipo => equipo.PartidosComoVisitante)
                .HasForeignKey(partido => partido.IdEquipoVisitante)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReprogramacionPartido>(builder =>
        {
            builder.ToTable("ReprogramacionesPartido");
            builder.HasKey(reprogramacion => reprogramacion.IdReprogramacion);
            builder.Property(reprogramacion => reprogramacion.Motivo).HasMaxLength(300);
            builder.Property(reprogramacion => reprogramacion.FechaRegistro).HasDefaultValueSql("SYSUTCDATETIME()");
            builder.HasOne(reprogramacion => reprogramacion.Partido)
                .WithMany(partido => partido.Reprogramaciones)
                .HasForeignKey(reprogramacion => reprogramacion.IdPartido)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }

    private static void ConfigureResultados(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resultado>(builder =>
        {
            builder.ToTable("Resultados", table =>
            {
                table.HasCheckConstraint("CK_Resultados_Sets", "SetsLocal >= 0 AND SetsVisitante >= 0");
                table.HasCheckConstraint("CK_Resultados_NoEmpate", "SetsLocal <> SetsVisitante");
                table.HasTrigger("TRG_Resultados_BloquearCampeonatoFinalizado");
                table.HasTrigger("TRG_Resultados_Auditoria_Update");
                table.HasTrigger("TRG_Resultados_RecalcularPosiciones");
            });
            builder.HasKey(resultado => resultado.IdResultado);
            builder.Property(resultado => resultado.FechaRegistro).HasDefaultValueSql("SYSUTCDATETIME()");
            builder.HasIndex(resultado => resultado.IdPartido).IsUnique().HasDatabaseName("UQ_Resultados_Partido");
            builder.HasOne(resultado => resultado.Partido)
                .WithOne(partido => partido.Resultado)
                .HasForeignKey<Resultado>(resultado => resultado.IdPartido)
                .OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(resultado => resultado.EquipoGanador)
                .WithMany(equipo => equipo.ResultadosGanados)
                .HasForeignKey(resultado => resultado.IdEquipoGanador)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ResultadoSet>(builder =>
        {
            builder.ToTable("ResultadoSets", table =>
            {
                table.HasCheckConstraint("CK_ResultadoSets_NumeroSet", "NumeroSet > 0");
                table.HasCheckConstraint("CK_ResultadoSets_Puntos", "PuntosLocal >= 0 AND PuntosVisitante >= 0");
                table.HasCheckConstraint("CK_ResultadoSets_NoEmpate", "PuntosLocal <> PuntosVisitante");
                table.HasTrigger("TRG_ResultadoSets_RecalcularPosiciones");
            });
            builder.HasKey(resultadoSet => resultadoSet.IdResultadoSet);
            builder.HasIndex(resultadoSet => new { resultadoSet.IdResultado, resultadoSet.NumeroSet })
                .IsUnique()
                .HasDatabaseName("UQ_ResultadoSets_Resultado_NumeroSet");
            builder.HasOne(resultadoSet => resultadoSet.Resultado)
                .WithMany(resultado => resultado.Sets)
                .HasForeignKey(resultadoSet => resultadoSet.IdResultado)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TablaPosicion>(builder =>
        {
            builder.ToTable("TablaPosiciones", table =>
            {
                table.HasCheckConstraint(
                    "CK_TablaPosiciones_Valores",
                    "PartidosJugados >= 0 AND Ganados >= 0 AND Perdidos >= 0 AND SetsFavor >= 0 AND SetsContra >= 0 AND PuntosFavor >= 0 AND PuntosContra >= 0 AND Puntos >= 0");
            });
            builder.HasKey(posicion => posicion.IdPosicion);
            builder.Property(posicion => posicion.PartidosJugados).HasDefaultValue(0);
            builder.Property(posicion => posicion.Ganados).HasDefaultValue(0);
            builder.Property(posicion => posicion.Perdidos).HasDefaultValue(0);
            builder.Property(posicion => posicion.SetsFavor).HasDefaultValue(0);
            builder.Property(posicion => posicion.SetsContra).HasDefaultValue(0);
            builder.Property(posicion => posicion.PuntosFavor).HasDefaultValue(0);
            builder.Property(posicion => posicion.PuntosContra).HasDefaultValue(0);
            builder.Property(posicion => posicion.Puntos).HasDefaultValue(0);
            builder.Property(posicion => posicion.FechaActualizacion).HasDefaultValueSql("SYSUTCDATETIME()");
            builder.HasIndex(posicion => new { posicion.IdCampeonatoCategoria, posicion.IdEquipo })
                .IsUnique()
                .HasDatabaseName("UQ_TablaPosiciones_CampeonatoCategoria_Equipo");
            builder.HasOne(posicion => posicion.CampeonatoCategoria)
                .WithMany(campeonatoCategoria => campeonatoCategoria.TablaPosiciones)
                .HasForeignKey(posicion => posicion.IdCampeonatoCategoria)
                .OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(posicion => posicion.Equipo)
                .WithMany(equipo => equipo.TablaPosiciones)
                .HasForeignKey(posicion => posicion.IdEquipo)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditoriaResultado>(builder =>
        {
            builder.ToTable("AuditoriaResultados");
            builder.HasKey(auditoria => auditoria.IdAuditoriaResultado);
            builder.Property(auditoria => auditoria.Motivo).HasMaxLength(300);
            builder.Property(auditoria => auditoria.FechaCambio).HasDefaultValueSql("SYSUTCDATETIME()");
            builder.HasOne(auditoria => auditoria.Resultado)
                .WithMany(resultado => resultado.Auditorias)
                .HasForeignKey(auditoria => auditoria.IdResultado)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
