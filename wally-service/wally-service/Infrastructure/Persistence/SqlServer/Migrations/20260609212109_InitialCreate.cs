using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WallyBallBackend.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Campeonatos",
                columns: table => new
                {
                    IdCampeonato = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "ACTIVO"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campeonatos", x => x.IdCampeonato);
                    table.CheckConstraint("CK_Campeonatos_Estado", "Estado IN ('BORRADOR', 'ACTIVO', 'FINALIZADO', 'CANCELADO')");
                    table.CheckConstraint("CK_Campeonatos_Fechas", "FechaFin IS NULL OR FechaFin >= FechaInicio");
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    IdRol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.IdRol);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.IdUsuario);
                });

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    IdCategoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCampeonato = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "ACTIVA"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.IdCategoria);
                    table.CheckConstraint("CK_Categorias_Estado", "Estado IN ('ACTIVA', 'INACTIVA')");
                    table.ForeignKey(
                        name: "FK_Categorias_Campeonatos_IdCampeonato",
                        column: x => x.IdCampeonato,
                        principalTable: "Campeonatos",
                        principalColumn: "IdCampeonato");
                });

            migrationBuilder.CreateTable(
                name: "Jugadores",
                columns: table => new
                {
                    IdJugador = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    Cedula = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jugadores", x => x.IdJugador);
                    table.ForeignKey(
                        name: "FK_Jugadores_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario");
                });

            migrationBuilder.CreateTable(
                name: "UsuarioRol",
                columns: table => new
                {
                    IdUsuarioRol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    IdRol = table.Column<int>(type: "int", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioRol", x => x.IdUsuarioRol);
                    table.ForeignKey(
                        name: "FK_UsuarioRol_Roles_IdRol",
                        column: x => x.IdRol,
                        principalTable: "Roles",
                        principalColumn: "IdRol");
                    table.ForeignKey(
                        name: "FK_UsuarioRol_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario");
                });

            migrationBuilder.CreateTable(
                name: "Equipos",
                columns: table => new
                {
                    IdEquipo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCategoria = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipos", x => x.IdEquipo);
                    table.ForeignKey(
                        name: "FK_Equipos_Categorias_IdCategoria",
                        column: x => x.IdCategoria,
                        principalTable: "Categorias",
                        principalColumn: "IdCategoria");
                });

            migrationBuilder.CreateTable(
                name: "Fases",
                columns: table => new
                {
                    IdFase = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCategoria = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "PENDIENTE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fases", x => x.IdFase);
                    table.CheckConstraint("CK_Fases_Estado", "Estado IN ('PENDIENTE', 'ACTIVA', 'FINALIZADA', 'CANCELADA')");
                    table.CheckConstraint("CK_Fases_Tipo", "Tipo IN ('ROUND_ROBIN', 'ELIMINATORIA')");
                    table.ForeignKey(
                        name: "FK_Fases_Categorias_IdCategoria",
                        column: x => x.IdCategoria,
                        principalTable: "Categorias",
                        principalColumn: "IdCategoria");
                });

            migrationBuilder.CreateTable(
                name: "InscripcionesEquipoJugador",
                columns: table => new
                {
                    IdInscripcion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEquipo = table.Column<int>(type: "int", nullable: false),
                    IdJugador = table.Column<int>(type: "int", nullable: false),
                    FechaInscripcion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "ACTIVO")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InscripcionesEquipoJugador", x => x.IdInscripcion);
                    table.CheckConstraint("CK_Inscripciones_Estado", "Estado IN ('ACTIVO', 'RETIRADO')");
                    table.ForeignKey(
                        name: "FK_InscripcionesEquipoJugador_Equipos_IdEquipo",
                        column: x => x.IdEquipo,
                        principalTable: "Equipos",
                        principalColumn: "IdEquipo");
                    table.ForeignKey(
                        name: "FK_InscripcionesEquipoJugador_Jugadores_IdJugador",
                        column: x => x.IdJugador,
                        principalTable: "Jugadores",
                        principalColumn: "IdJugador");
                });

            migrationBuilder.CreateTable(
                name: "TablaPosiciones",
                columns: table => new
                {
                    IdPosicion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCategoria = table.Column<int>(type: "int", nullable: false),
                    IdEquipo = table.Column<int>(type: "int", nullable: false),
                    PartidosJugados = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Ganados = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Perdidos = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SetsFavor = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SetsContra = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PuntosFavor = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PuntosContra = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Puntos = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TablaPosiciones", x => x.IdPosicion);
                    table.CheckConstraint("CK_TablaPosiciones_Valores", "PartidosJugados >= 0 AND Ganados >= 0 AND Perdidos >= 0 AND SetsFavor >= 0 AND SetsContra >= 0 AND PuntosFavor >= 0 AND PuntosContra >= 0 AND Puntos >= 0");
                    table.ForeignKey(
                        name: "FK_TablaPosiciones_Categorias_IdCategoria",
                        column: x => x.IdCategoria,
                        principalTable: "Categorias",
                        principalColumn: "IdCategoria");
                    table.ForeignKey(
                        name: "FK_TablaPosiciones_Equipos_IdEquipo",
                        column: x => x.IdEquipo,
                        principalTable: "Equipos",
                        principalColumn: "IdEquipo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Jornadas",
                columns: table => new
                {
                    IdJornada = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdFase = table.Column<int>(type: "int", nullable: false),
                    NumeroJornada = table.Column<int>(type: "int", nullable: false),
                    FechaProgramada = table.Column<DateOnly>(type: "date", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "PROGRAMADA")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jornadas", x => x.IdJornada);
                    table.CheckConstraint("CK_Jornadas_Estado", "Estado IN ('PROGRAMADA', 'FINALIZADA', 'CANCELADA')");
                    table.CheckConstraint("CK_Jornadas_Numero", "NumeroJornada > 0");
                    table.ForeignKey(
                        name: "FK_Jornadas_Fases_IdFase",
                        column: x => x.IdFase,
                        principalTable: "Fases",
                        principalColumn: "IdFase");
                });

            migrationBuilder.CreateTable(
                name: "Partidos",
                columns: table => new
                {
                    IdPartido = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCategoria = table.Column<int>(type: "int", nullable: false),
                    IdFase = table.Column<int>(type: "int", nullable: false),
                    IdJornada = table.Column<int>(type: "int", nullable: false),
                    IdEquipoLocal = table.Column<int>(type: "int", nullable: false),
                    IdEquipoVisitante = table.Column<int>(type: "int", nullable: false),
                    FechaHoraProgramada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "PROGRAMADO"),
                    EquipoMayor = table.Column<int>(type: "int", nullable: false, computedColumnSql: "CASE WHEN IdEquipoLocal > IdEquipoVisitante THEN IdEquipoLocal ELSE IdEquipoVisitante END", stored: true),
                    EquipoMenor = table.Column<int>(type: "int", nullable: false, computedColumnSql: "CASE WHEN IdEquipoLocal < IdEquipoVisitante THEN IdEquipoLocal ELSE IdEquipoVisitante END", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partidos", x => x.IdPartido);
                    table.CheckConstraint("CK_Partidos_EquiposDiferentes", "IdEquipoLocal <> IdEquipoVisitante");
                    table.CheckConstraint("CK_Partidos_Estado", "Estado IN ('PROGRAMADO', 'REPROGRAMADO', 'FINALIZADO', 'CANCELADO')");
                    table.ForeignKey(
                        name: "FK_Partidos_Categorias_IdCategoria",
                        column: x => x.IdCategoria,
                        principalTable: "Categorias",
                        principalColumn: "IdCategoria",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Partidos_Equipos_IdEquipoLocal",
                        column: x => x.IdEquipoLocal,
                        principalTable: "Equipos",
                        principalColumn: "IdEquipo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Partidos_Equipos_IdEquipoVisitante",
                        column: x => x.IdEquipoVisitante,
                        principalTable: "Equipos",
                        principalColumn: "IdEquipo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Partidos_Fases_IdFase",
                        column: x => x.IdFase,
                        principalTable: "Fases",
                        principalColumn: "IdFase",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Partidos_Jornadas_IdJornada",
                        column: x => x.IdJornada,
                        principalTable: "Jornadas",
                        principalColumn: "IdJornada",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReprogramacionesPartido",
                columns: table => new
                {
                    IdReprogramacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPartido = table.Column<int>(type: "int", nullable: false),
                    FechaHoraAnterior = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaHoraNueva = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReprogramacionesPartido", x => x.IdReprogramacion);
                    table.ForeignKey(
                        name: "FK_ReprogramacionesPartido_Partidos_IdPartido",
                        column: x => x.IdPartido,
                        principalTable: "Partidos",
                        principalColumn: "IdPartido");
                });

            migrationBuilder.CreateTable(
                name: "Resultados",
                columns: table => new
                {
                    IdResultado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPartido = table.Column<int>(type: "int", nullable: false),
                    SetsLocal = table.Column<int>(type: "int", nullable: false),
                    SetsVisitante = table.Column<int>(type: "int", nullable: false),
                    IdEquipoGanador = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resultados", x => x.IdResultado);
                    table.CheckConstraint("CK_Resultados_NoEmpate", "SetsLocal <> SetsVisitante");
                    table.CheckConstraint("CK_Resultados_Sets", "SetsLocal >= 0 AND SetsVisitante >= 0");
                    table.ForeignKey(
                        name: "FK_Resultados_Equipos_IdEquipoGanador",
                        column: x => x.IdEquipoGanador,
                        principalTable: "Equipos",
                        principalColumn: "IdEquipo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Resultados_Partidos_IdPartido",
                        column: x => x.IdPartido,
                        principalTable: "Partidos",
                        principalColumn: "IdPartido");
                });

            migrationBuilder.CreateTable(
                name: "AuditoriaResultados",
                columns: table => new
                {
                    IdAuditoriaResultado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdResultado = table.Column<int>(type: "int", nullable: false),
                    IdPartido = table.Column<int>(type: "int", nullable: false),
                    SetsLocalAnterior = table.Column<int>(type: "int", nullable: false),
                    SetsVisitanteAnterior = table.Column<int>(type: "int", nullable: false),
                    IdEquipoGanadorAnterior = table.Column<int>(type: "int", nullable: false),
                    SetsLocalNuevo = table.Column<int>(type: "int", nullable: false),
                    SetsVisitanteNuevo = table.Column<int>(type: "int", nullable: false),
                    IdEquipoGanadorNuevo = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FechaCambio = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriaResultados", x => x.IdAuditoriaResultado);
                    table.ForeignKey(
                        name: "FK_AuditoriaResultados_Resultados_IdResultado",
                        column: x => x.IdResultado,
                        principalTable: "Resultados",
                        principalColumn: "IdResultado");
                });

            migrationBuilder.CreateTable(
                name: "ResultadoSets",
                columns: table => new
                {
                    IdResultadoSet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdResultado = table.Column<int>(type: "int", nullable: false),
                    NumeroSet = table.Column<int>(type: "int", nullable: false),
                    PuntosLocal = table.Column<int>(type: "int", nullable: false),
                    PuntosVisitante = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultadoSets", x => x.IdResultadoSet);
                    table.CheckConstraint("CK_ResultadoSets_NoEmpate", "PuntosLocal <> PuntosVisitante");
                    table.CheckConstraint("CK_ResultadoSets_NumeroSet", "NumeroSet > 0");
                    table.CheckConstraint("CK_ResultadoSets_Puntos", "PuntosLocal >= 0 AND PuntosVisitante >= 0");
                    table.ForeignKey(
                        name: "FK_ResultadoSets_Resultados_IdResultado",
                        column: x => x.IdResultado,
                        principalTable: "Resultados",
                        principalColumn: "IdResultado",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "IdRol", "Activo", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "Usuario encargado de administrar campeonatos, categorias, equipos, jugadores, fixture, resultados y posiciones.", "ORGANIZADOR" },
                    { 2, true, "Usuario que puede consultar fixture, resultados, posiciones e informacion de su equipo.", "JUGADOR" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaResultados_IdResultado",
                table: "AuditoriaResultados",
                column: "IdResultado");

            migrationBuilder.CreateIndex(
                name: "UQ_Categorias_Campeonato_Nombre",
                table: "Categorias",
                columns: new[] { "IdCampeonato", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Equipos_Categoria_Nombre",
                table: "Equipos",
                columns: new[] { "IdCategoria", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Fases_Categoria_Orden",
                table: "Fases",
                columns: new[] { "IdCategoria", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InscripcionesEquipoJugador_IdJugador",
                table: "InscripcionesEquipoJugador",
                column: "IdJugador");

            migrationBuilder.CreateIndex(
                name: "UQ_Inscripciones_Equipo_Jugador",
                table: "InscripcionesEquipoJugador",
                columns: new[] { "IdEquipo", "IdJugador" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Jornadas_Fase_Numero",
                table: "Jornadas",
                columns: new[] { "IdFase", "NumeroJornada" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Jugadores_Cedula",
                table: "Jugadores",
                column: "Cedula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Jugadores_IdUsuario",
                table: "Jugadores",
                column: "IdUsuario",
                unique: true,
                filter: "[IdUsuario] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Partidos_IdCategoria",
                table: "Partidos",
                column: "IdCategoria");

            migrationBuilder.CreateIndex(
                name: "IX_Partidos_IdEquipoLocal",
                table: "Partidos",
                column: "IdEquipoLocal");

            migrationBuilder.CreateIndex(
                name: "IX_Partidos_IdEquipoVisitante",
                table: "Partidos",
                column: "IdEquipoVisitante");

            migrationBuilder.CreateIndex(
                name: "IX_Partidos_IdFase_IdEquipoLocal_IdEquipoVisitante",
                table: "Partidos",
                columns: new[] { "IdFase", "IdEquipoLocal", "IdEquipoVisitante" });

            migrationBuilder.CreateIndex(
                name: "IX_Partidos_IdJornada",
                table: "Partidos",
                column: "IdJornada");

            migrationBuilder.CreateIndex(
                name: "UQ_Partidos_Fase_Enfrentamiento",
                table: "Partidos",
                columns: new[] { "IdFase", "EquipoMenor", "EquipoMayor" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReprogramacionesPartido_IdPartido",
                table: "ReprogramacionesPartido",
                column: "IdPartido");

            migrationBuilder.CreateIndex(
                name: "IX_Resultados_IdEquipoGanador",
                table: "Resultados",
                column: "IdEquipoGanador");

            migrationBuilder.CreateIndex(
                name: "UQ_Resultados_Partido",
                table: "Resultados",
                column: "IdPartido",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ResultadoSets_Resultado_NumeroSet",
                table: "ResultadoSets",
                columns: new[] { "IdResultado", "NumeroSet" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Roles_Nombre",
                table: "Roles",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TablaPosiciones_IdEquipo",
                table: "TablaPosiciones",
                column: "IdEquipo");

            migrationBuilder.CreateIndex(
                name: "UQ_TablaPosiciones_Categoria_Equipo",
                table: "TablaPosiciones",
                columns: new[] { "IdCategoria", "IdEquipo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRol_IdRol",
                table: "UsuarioRol",
                column: "IdRol");

            migrationBuilder.CreateIndex(
                name: "UQ_UsuarioRol_Usuario_Rol",
                table: "UsuarioRol",
                columns: new[] { "IdUsuario", "IdRol" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditoriaResultados");

            migrationBuilder.DropTable(
                name: "InscripcionesEquipoJugador");

            migrationBuilder.DropTable(
                name: "ReprogramacionesPartido");

            migrationBuilder.DropTable(
                name: "ResultadoSets");

            migrationBuilder.DropTable(
                name: "TablaPosiciones");

            migrationBuilder.DropTable(
                name: "UsuarioRol");

            migrationBuilder.DropTable(
                name: "Jugadores");

            migrationBuilder.DropTable(
                name: "Resultados");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Partidos");

            migrationBuilder.DropTable(
                name: "Equipos");

            migrationBuilder.DropTable(
                name: "Jornadas");

            migrationBuilder.DropTable(
                name: "Fases");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Campeonatos");
        }
    }
}
